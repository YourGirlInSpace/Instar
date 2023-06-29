using System.Collections.Concurrent;
using System.Runtime.Caching;
using System.Timers;
using Discord;
using Discord.WebSocket;
using Microsoft.Extensions.Configuration;
using PaxAndromeda.Instar.Caching;
using PaxAndromeda.Instar.ConfigModels;
using PaxAndromeda.Instar.Metrics;
using Serilog;
using Timer = System.Timers.Timer;

namespace PaxAndromeda.Instar.Services;

public sealed class AutoMemberSystem
{
    private readonly MemoryCache _ddbCache = new("AutoMemberSystem_DDBCache");
    private readonly MemoryCache<MessageProperties> _messageCache = new("AutoMemberSystem_MessageCache");
    private readonly ConcurrentDictionary<ulong, bool> _introductionPosters = new();
    private readonly ConcurrentDictionary<ulong, bool> _punishedUsers = new();
    
    [SnowflakeType(SnowflakeType.Role)] private readonly Snowflake _newMemberRole;
    [SnowflakeType(SnowflakeType.Role)] private readonly Snowflake _memberRole;
    [SnowflakeType(SnowflakeType.Role)] private readonly Snowflake _holdRole;
    private readonly AutoMemberConfig _amsConfig;
    private DateTime _earliestJoinTime;
    
    private readonly IDiscordService _discord;
    private readonly IGaiusAPIService _gaiusApiService;
    private readonly IInstarDDBService _ddbService;
    private readonly IMetricService _metricService;
    private Timer _timer = null!;

    /// <summary>
    /// Recent messages per the last AMS run
    /// </summary>
    private Dictionary<ulong, int>? _recentMessages;

    public AutoMemberSystem(IConfiguration config, IDiscordService discord, IGaiusAPIService gaiusApiService,
        IInstarDDBService ddbService, IMetricService metricService)
    {
        _newMemberRole = config.GetValue<ulong>("NewMemberRoleID");
        _memberRole = config.GetValue<ulong>("MemberRoleID");
        _amsConfig = config.GetSection("AutoMemberConfig").Get<AutoMemberConfig>()!;
        _holdRole = _amsConfig.HoldRole;

        _discord = discord;
        _gaiusApiService = gaiusApiService;
        _ddbService = ddbService;
        _metricService = metricService;
        _earliestJoinTime = DateTime.UtcNow - TimeSpan.FromSeconds(_amsConfig.MinimumJoinAge);

        discord.UserJoined += HandleUserJoined;
        discord.MessageReceived += HandleMessageReceived;
        discord.MessageDeleted += HandleMessageDeleted;

        // Preload our message cache
        Task.Run(PreloadMessageCache).Wait();
        Task.Run(PreloadIntroductionPosters).Wait();
        if (_amsConfig.EnableGaiusCheck)
            Task.Run(PreloadGaiusPunishments).Wait();
        StartTimer();
    }

    private async Task UpdateGaiusPunishments()
    {
        // Normally we'd go for 1 hour here, but we can run into
        // a situation where someone was warned exactly 1.000000001
        // hours ago, thus would be missed.  To fix this, we'll
        // bias for an hour and a half ago.
        var afterTime = DateTime.UtcNow - TimeSpan.FromHours(1.5);
        
        foreach (var warning in await _gaiusApiService.GetWarningsAfter(afterTime))
            _punishedUsers.TryAdd(warning.UserID.ID, true);
        foreach (var caselog in await _gaiusApiService.GetCaselogsAfter(afterTime))
            _punishedUsers.TryAdd(caselog.UserID.ID, true);
    }

    private async Task HandleMessageDeleted(Snowflake arg)
    {
        await _metricService.Emit(Metric.Discord_MessagesDeleted, 1);
        
        if (!_messageCache.Contains(arg.ID.ToString()))
            return;

        _messageCache.Remove(arg.ID.ToString());
        await _metricService.Emit(Metric.AMS_CachedMessages, _messageCache.GetCount());
    }

    private async Task HandleMessageReceived(IMessage arg)
    {
        ulong guildId = 0;
        if (arg.Author is SocketGuildUser guildUser)
            guildId = guildUser.Guild.Id;
        
        var mp = new MessageProperties(arg.Author.Id, arg.Channel.Id, guildId);
        _messageCache.Add(arg.Id.ToString(), mp, new CacheItemPolicy
        {
            AbsoluteExpiration = DateTimeOffset.UtcNow + TimeSpan.FromSeconds(_amsConfig.MinimumMessageTime)
        });

        await _metricService.Emit(Metric.Discord_MessagesSent, 1);
        await _metricService.Emit(Metric.AMS_CachedMessages, _messageCache.GetCount());

        if (!arg.Channel.Id.Equals(_amsConfig.IntroductionChannel.ID)) 
            return;
        
        // Ignore members
        if (arg.Author is IGuildUser sgUser && sgUser.RoleIds.Contains(_memberRole.ID))
            return;
            
        _introductionPosters.TryAdd(arg.Author.Id, true);
    }

    private async Task HandleUserJoined(IGuildUser user)
    {
        if (await WasUserGrantedMembershipBefore(user.Id))
        {
            Log.Information("User {UserID} has been granted membership before.  Granting membership again", user.Id);
            await GrantMembership(user);
        }
        await _metricService.Emit(Metric.Discord_UsersJoined, 1);
    }

    private void StartTimer()
    {
        // Since we can start the bot in the middle of an hour,
        // first we must determine the time until the next top
        // of hour.
        var nextHour = new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, DateTime.UtcNow.Day,
            DateTime.UtcNow.Hour, 0, 0).AddHours(1);
        var millisecondsRemaining = (nextHour - DateTime.UtcNow).TotalMilliseconds;
        
        // Start the timer.  In elapsed step, we reset the
        // duration to exactly 1 hour.
        _timer = new Timer(millisecondsRemaining);
        _timer.Elapsed += TimerElapsed;
        _timer.Start();
    }

    private async void TimerElapsed(object? sender, ElapsedEventArgs e)
    {
        // Ensure the timer's interval is exactly 1 hour
        _timer.Interval = 60 * 60 * 1000;

        await RunAsync();
    }
    
    public async Task RunAsync()
    {
        try
        {
            await _metricService.Emit(Metric.AMS_Runs, 1);
            
            // Caution:  This is an extremely long running method!
            Log.Information("Beginning auto member routine");

            if (_amsConfig.EnableGaiusCheck)
            {
                Log.Information("Updating Gaius database");
                await UpdateGaiusPunishments();
            }

            _earliestJoinTime = DateTime.UtcNow - TimeSpan.FromSeconds(_amsConfig.MinimumJoinAge);
            _recentMessages = GetMessagesSent();
            
            Log.Verbose("Earliest join time: {EarliestJoinTime}", _earliestJoinTime);

            var users = await _discord.GetAllUsers();
            
            // Filter for new members that joined more than 1 day ago and have the correct roles
            var newMembers = users
                .Where(user => user.RoleIds.Contains(_newMemberRole)).ToList();
            
            await _metricService.Emit(Metric.AMS_NewMembers, newMembers.Count);
            Log.Verbose("There are {NumNewMembers} users with the New Member role", newMembers.Count);

            var membershipGrants = 0;

            newMembers = newMembers
                .Where(user => CheckEligibility(user) == MembershipEligibility.Eligible).ToList();
            
            Log.Verbose("There are {NumNewMembers} users eligible for membership", newMembers.Count);
                
            foreach (var user in newMembers)
            {
                // User has all of the qualifications, let's update their role
                try
                {
                    await GrantMembership(user);
                    membershipGrants++;
                    
                    Log.Information("Granted {UserId} membership", user.Id);
                }
                catch (Exception ex)
                {
                    Log.Warning(ex, "Failed to grant user {UserId} membership", user.Id);
                }
            }
            
            await _metricService.Emit(Metric.AMS_UsersGrantedMembership, membershipGrants);
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Auto member system failed to run");
        }
    }

    private async Task<bool> WasUserGrantedMembershipBefore(Snowflake snowflake)
    {
        if (_ddbCache.Contains(snowflake.ID.ToString()) && (bool)_ddbCache[snowflake.ID.ToString()])
            return true;

        var grantedMembership = await _ddbService.GetUserMembership(snowflake.ID);
        if (grantedMembership is null)
            return false;
        
        // Cache for 6 hour sliding window.  If accessed, time is reset.
        _ddbCache.Add(snowflake.ID.ToString(), grantedMembership.Value, new CacheItemPolicy
        {
            SlidingExpiration = TimeSpan.FromHours(6)
        });

        return grantedMembership.Value;
    }

    private async Task GrantMembership(IGuildUser user)
    {
        await user.AddRoleAsync(_memberRole);
        await user.RemoveRoleAsync(_newMemberRole);
        await _ddbService.UpdateUserMembership(user.Id, true);

        // Remove the cache entry
        if (_ddbCache.Contains(user.Id.ToString()))
            _ddbCache.Remove(user.Id.ToString());
        
        // Remove introduction reference, if it exists
        _introductionPosters.TryRemove(user.Id, out _);
    }

    public MembershipEligibility CheckEligibility(IGuildUser user)
    {
        // We need recent messages here, so load it into
        // context if it does not exist, such as when the
        // bot first starts and has not run AMS yet.
        _recentMessages = GetMessagesSent();

        var eligibility = MembershipEligibility.Eligible;

        if (user.RoleIds.Contains(_memberRole.ID))
            eligibility |= MembershipEligibility.AlreadyMember;

        if (user.JoinedAt > _earliestJoinTime)
            eligibility |= MembershipEligibility.TooYoung;

        if (!CheckUserRequiredRoles(user))
            eligibility |= MembershipEligibility.MissingRoles;

        if (!_introductionPosters.ContainsKey(user.Id))
            eligibility |=  MembershipEligibility.MissingIntroduction;

        if (!_recentMessages.ContainsKey(user.Id) || _recentMessages[user.Id] < _amsConfig.MinimumMessages)
            eligibility |=  MembershipEligibility.NotEnoughMessages;

        if (_punishedUsers.ContainsKey(user.Id))
            eligibility |= MembershipEligibility.PunishmentReceived;

        if (eligibility != MembershipEligibility.Eligible)
            eligibility |= MembershipEligibility.NotEligible;
        
        Log.Verbose("User {User} ({UserID}) membership eligibility: {Eligibility}", user.Username, user.Id, eligibility);
        return eligibility;
    }

    private bool CheckUserRequiredRoles(IGuildUser user)
    {
        // Auto Member Hold overrides all role permissions
        if (user.RoleIds.Contains(_holdRole))
            return false;
        
        return _amsConfig.RequiredRoles.All(
            roleGroup => roleGroup.Roles.Select(n => n.ID)
                .Intersect(user.RoleIds).Any()
            );
    }

    private Dictionary<ulong, int> GetMessagesSent()
    {
        var map = new Dictionary<ulong, int>();

        foreach (var cacheEntry in _messageCache)
        {
            if (!map.ContainsKey(cacheEntry.Value.UserID))
                map.Add(cacheEntry.Value.UserID, 1);
            else
                map[cacheEntry.Value.UserID]++;
        }

        return map;
    }
    
    private async Task PreloadGaiusPunishments()
    {
        foreach (var warning in await _gaiusApiService.GetAllWarnings())
            _punishedUsers.TryAdd(warning.UserID.ID, true);
        foreach (var caselog in await _gaiusApiService.GetAllCaselogs())
            _punishedUsers.TryAdd(caselog.UserID.ID, true);
    }

    private async Task PreloadMessageCache()
    {
        Log.Information("Preloading message cache...");
        var guild = _discord.GetGuild();
        var earliestMessageTime = DateTime.UtcNow - TimeSpan.FromSeconds(_amsConfig.MinimumMessageTime);
        var messages = _discord.GetMessages(guild, earliestMessageTime);

        await foreach (var message in messages)
        {
            var mp = new MessageProperties(message.Author.Id, message.Channel?.Id ?? 0, guild.Id);
            _messageCache.Add(message.Id.ToString(), mp, new CacheItemPolicy
            {
                AbsoluteExpiration = message.Timestamp + TimeSpan.FromSeconds(_amsConfig.MinimumMessageTime)
            });
        }
        Log.Information("Done preloading message cache!");
    }

    private async Task PreloadIntroductionPosters()
    {
        if (await _discord.GetChannel(_amsConfig.IntroductionChannel) is not ITextChannel introChannel)
            throw new InvalidOperationException("Introductions channel not found");
        
        var messages = (await introChannel.GetMessagesAsync().FlattenAsync()).ToList();

        // Assumption:  Last message is the oldest one
        while (messages.Count > 0)
        {
            var oldestMessage = messages[0];
            foreach (var message in messages)
            {
                if (message.Author is IGuildUser sgUser && sgUser.RoleIds.Contains(_memberRole.ID))
                    continue;
                
                _introductionPosters.TryAdd(message.Author.Id, true);
                if (message.Timestamp < oldestMessage.Timestamp)
                    oldestMessage = message;
            }

            messages = (await introChannel.GetMessagesAsync(oldestMessage, Direction.Before).FlattenAsync()).ToList();
        }
    }
}