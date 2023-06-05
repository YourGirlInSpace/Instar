using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using JetBrains.Annotations;
using Serilog;

namespace PaxAndromeda.Instar.Commands;

public class SetBirthdayCommand : InteractionModuleBase<SocketInteractionContext>
{
    /// <summary>
    ///     Simple mapping from month number to month name.
    /// </summary>
    private static readonly Dictionary<int, string> MonthNameMap = new()
    {
        { 1, "January" },
        { 2, "February" },
        { 3, "March" },
        { 4, "April" },
        { 5, "May" },
        { 6, "June" },
        { 7, "July" },
        { 8, "August" },
        { 9, "September" },
        { 10, "October" },
        { 11, "November" },
        { 12, "December" }
    };


    [UsedImplicitly]
    [RequireOwner]
    [DefaultMemberPermissions(GuildPermission.Administrator)]
    [SlashCommand("setbirthday", "Sets your birthday on the server.")]
    public async Task SetBirthday(
        [MinValue(1)] [MaxValue(12)] [Summary(description: "The month you were born.")]
        Month month,
        [MinValue(1)] [MaxValue(31)] [Summary(description: "The day you were born.")]
        int day,
        [MinValue(1900)] [MaxValue(2099)] [Summary(description: "The year you were born.")]
        int year,
        [MinValue(-12)]
        [MaxValue(+12)]
        [Summary("timezone", "Select your nearest time zone offset in hours from GMT.")]
        [Autocomplete]
        int tzOffset = 0)
    {
        var daysInMonth = DateTime.DaysInMonth(year, (int)month);

        // First step:  Does the provided number of days exceed the number of days in the given month?
        if (day > daysInMonth)
        {
            await RespondAsync(
                $"There are only {daysInMonth} days in {MonthNameMap[(int)month]} {year}.  Your birthday was not set.",
                ephemeral: true);
            return;
        }

        var dtLocal = new DateTime(year, (int)month, day, 0, 0, 0, DateTimeKind.Unspecified);
        var dtUtc = new DateTime(year, (int)month, day, 0, 0, 0, DateTimeKind.Utc).AddHours(-tzOffset);

        // Second step:  Is the provided birthday actually in the future?
        if (dtUtc > DateTime.UtcNow)
        {
            await RespondAsync(
                "You are not a time traveler.  Your birthday was not set.",
                ephemeral: true);
            return;
        }

        // Third step:  Is the user below the age of 13?
        // Note:  We will assume all years are 365.25 days to account for leap year madness.
        if (DateTime.UtcNow - dtUtc < TimeSpan.FromDays(365.25 * 13))
            Log.Warning("User {UserID} recorded a birthday that puts their age below 13!  {UTCTime}", Context.User.Id,
                dtUtc);
        // TODO:  Notify staff?
        Log.Information("User {UserID} birthday set to {DateTime} (UTC time calculated as {UTCTime})", Context.User.Id,
            dtLocal, dtUtc);

        await RespondAsync($"Your birthday was set to {dtLocal:D}.", ephemeral: true);
    }

    [UsedImplicitly]
    [AutocompleteCommand("timezone", "setbirthday")]
    public async Task HandleTimezoneAutocomplete()
    {
        Log.Debug("AUTOCOMPLETE");
        IEnumerable<AutocompleteResult> results = new[]
        {
            new AutocompleteResult("GMT-12 International Date Line West", -12),
            new AutocompleteResult("GMT-11 Midway Island, Samoa", -11),
            new AutocompleteResult("GMT-10 Hawaii", -10),
            new AutocompleteResult("GMT-9 Alaska", -9),
            new AutocompleteResult("GMT-8 Pacific Time (US and Canada); Tijuana", -8),
            new AutocompleteResult("GMT-7 Mountain Time (US and Canada)", -7),
            new AutocompleteResult("GMT-6 Central Time (US and Canada)", -6),
            new AutocompleteResult("GMT-5 Eastern Time (US and Canada)", -5),
            new AutocompleteResult("GMT-4 Atlantic Time (Canada)", -4),
            new AutocompleteResult("GMT-3 Brasilia, Buenos Aires, Georgetown", -3),
            new AutocompleteResult("GMT-2 Mid-Atlantic", -2),
            new AutocompleteResult("GMT-1 Azores, Cape Verde Islands", -1),
            new AutocompleteResult("GMT+0 Greenwich Mean Time: Dublin, Edinburgh, Lisbon, London", 0),
            new AutocompleteResult("GMT+1 Amsterdam, Berlin, Bern, Rome, Stockholm, Vienna", 1),
            new AutocompleteResult("GMT+2 Helsinki, Kiev, Riga, Sofia, Tallinn, Vilnius", 2),
            new AutocompleteResult("GMT+3 Moscow, St. Petersburg, Volgograd", 3),
            new AutocompleteResult("GMT+4 Abu Dhabi, Muscat", 4),
            new AutocompleteResult("GMT+5 Islamabad, Karachi, Tashkent", 5),
            new AutocompleteResult("GMT+6 Astana, Dhaka", 6),
            new AutocompleteResult("GMT+7 Bangkok, Hanoi, Jakarta", 7),
            new AutocompleteResult("GMT+8 Beijing, Chongqing, Hong Kong SAR, Urumqi", 8),
            new AutocompleteResult("GMT+9 Seoul, Osaka, Sapporo, Tokyo", 9),
            new AutocompleteResult("GMT+10 Canberra, Melbourne, Sydney", 10),
            new AutocompleteResult("GMT+11 Magadan, Solomon Islands, New Caledonia", 11),
            new AutocompleteResult("GMT+12 Auckland, Wellington", 12)
        };

        await (Context.Interaction as SocketAutocompleteInteraction)?.RespondAsync(results)!;
    }
}