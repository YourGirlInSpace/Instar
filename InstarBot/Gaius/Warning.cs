using JetBrains.Annotations;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace PaxAndromeda.Instar.Gaius;

[UsedImplicitly]
public record Warning
{
    [UsedImplicitly] public Snowflake GuildID { get; set; } = default!;
    [UsedImplicitly] public int WarnID { get; set; }
    [UsedImplicitly] public Snowflake UserID { get; set; } = default!;
    [UsedImplicitly] public string Reason { get; set; } = default!;

    [JsonConverter(typeof(UnixDateTimeConverter)), UsedImplicitly]
    public DateTime WarnDate { get; set; }

    [UsedImplicitly] public Snowflake? PardonerID { get; set; }

    [JsonConverter(typeof(UnixDateTimeConverter)), UsedImplicitly]
    public DateTime? PardonDate { get; set; }

    [UsedImplicitly] public Snowflake ModID { get; set; } = default!;
}