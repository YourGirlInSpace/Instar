using Discord;
using Microsoft.Extensions.Configuration;
using PaxAndromeda.Instar.ConfigModels;
using Serilog;

namespace PaxAndromeda.Instar.Services;

public class AutoMemberSystem
{
    [SnowflakeType(SnowflakeType.Role)] private readonly Snowflake _newMemberRole;
    [SnowflakeType(SnowflakeType.Role)] private readonly Snowflake _memberRole;
    [SnowflakeType(SnowflakeType.Role)] private readonly Snowflake _holdRole;
    private readonly AutoMemberConfig _amsConfig;
    
    private readonly IDiscordService _discord;
    private readonly IGaiusAPIService _gaiusApiService;

    public AutoMemberSystem(IConfiguration config, IDiscordService discord, IGaiusAPIService gaiusApiService)
    {
        _newMemberRole = config.GetValue<ulong>("NewMemberRoleID");
        _memberRole = config.GetValue<ulong>("MemberRoleID");
        _amsConfig = config.GetSection("AutoMemberConfig").Get<AutoMemberConfig>()!;
        _holdRole = _amsConfig.HoldRole;
        
        _discord = discord;
        _gaiusApiService = gaiusApiService;
    }
    
    public async Task RunAsync()
    {
        try
        {
            // Caution:  This is an extremely long running method!
            Log.Information("Beginning auto member routine");

            var earliestJoinTime = DateTime.UtcNow - TimeSpan.FromSeconds(_amsConfig.MinimumJoinAge);
            var earliestMessageTime = DateTime.UtcNow - TimeSpan.FromDays(_amsConfig.MinimumMessageTime);

            var users = await _discord.GetAllUsers();
            
            // Filter for new members that joined more than 1 day ago and have the correct roles
            var newMembers = users
                .Where(user => user.RoleIds.Contains(_newMemberRole))
                .Where(user => user.JoinedAt < earliestJoinTime)
                .Where(CheckUserRequiredRoles)
                .ToList();

            if (!newMembers.Any())
            {
                Log.Information("No new members are eligible for membership at this time");
                return;
            }

            var earliestUser = newMembers.OrderBy(user => user.JoinedAt).First();

            var introductionPosters = await GetIntroductionPosters(earliestUser.JoinedAt);
            var recentMessages = await GetMessagesSent(_discord.GetGuild(), earliestMessageTime);

            // Filter for eligible members
            newMembers = newMembers
                .Where(user => introductionPosters.Contains(user.Id))
                .Where(user => recentMessages.ContainsKey(user.Id) && recentMessages[user.Id] >= _amsConfig.MinimumMessages)
                .ToList();

            if (!newMembers.Any())
            {
                Log.Information("No new members are eligible for membership at this time");
                return;
            }
            
            foreach (var user in newMembers)
            {
                // We only want to poll Gaius if we're sure a user is otherwise
                // eligible for membership.
                var warnings = await _gaiusApiService.GetWarnings(user.Id);
                if (warnings is null || warnings.Any())
                    continue;
                
                var caselogs = await _gaiusApiService.GetCaselogs(user.Id);
                if (caselogs is null || caselogs.Any())
                    continue;
                
                // User has all of the qualifications, let's update their role
                try
                {
                    await user.AddRoleAsync(_memberRole);
                    await user.RemoveRoleAsync(_newMemberRole);
                    
                    Log.Information("Granted {UserId} membership", user.Id);
                }
                catch (Exception ex)
                {
                    Log.Warning(ex, "Failed to grant user {UserId} membership", user.Id);
                }
            }
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Auto member system failed to run");
        }
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

    private async Task<Dictionary<ulong, int>> GetMessagesSent(IInstarGuild guild, DateTime afterTime)
    {
        var newMemberRole = guild.GetRole(_newMemberRole);
        
        var map = new Dictionary<ulong, int>();

        foreach (var channel in guild.TextChannels)
        {
            var overwrite = channel.GetPermissionOverwrite(newMemberRole);
            if (overwrite.HasValue && !(overwrite.Value.ViewChannel.HasFlag(PermValue.Allow) ||
                                       overwrite.Value.ViewChannel.HasFlag(PermValue.Inherit)))
            {
                // New members don't have access to this channel,
                // so it's pointless to check
                Log.Verbose("Skipping channel {ChannelId} because new members don't have access", channel.Id);
                continue;
            }
            
            var messages = (await channel.GetMessagesAsync().FlattenAsync()).ToList();
            
            while (messages.Count > 0)
            {
                var oldestMessage = messages[0];
                var done = false;
                foreach (var message in messages)
                {
                    if (!map.ContainsKey(message.Author.Id))
                        map.Add(message.Author.Id, 1);
                    else
                        map[message.Author.Id]++;

                    if (message.Timestamp < afterTime)
                    {
                        done = true;
                        break;
                    }

                    if (message.Timestamp < oldestMessage.Timestamp)
                        oldestMessage = message;
                }

                if (done)
                    break;

                messages = (await channel.GetMessagesAsync(oldestMessage, Direction.Before).FlattenAsync()).ToList();
            }
        }

        return map;
    }

    private async Task<HashSet<ulong>> GetIntroductionPosters(DateTimeOffset? earliestTime)
    {
        if (await _discord.GetChannel(_amsConfig.IntroductionChannel) is not ITextChannel introChannel)
            throw new InvalidOperationException("Introductions channel not found");

        var introductionPosters = new HashSet<ulong>();
        var messages = (await introChannel.GetMessagesAsync().FlattenAsync()).ToList();

        // Assumption:  Last message is the oldest one
        while (messages.Count > 0)
        {
            var oldestMessage = messages[0];
            foreach (var message in messages)
            {
                introductionPosters.Add(message.Author.Id);
                if (message.Timestamp < earliestTime)
                    return introductionPosters;
                if (message.Timestamp < oldestMessage.Timestamp)
                    oldestMessage = message;
            }

            messages = (await introChannel.GetMessagesAsync(oldestMessage, Direction.Before).FlattenAsync()).ToList();
        }

        return introductionPosters;
    }
}