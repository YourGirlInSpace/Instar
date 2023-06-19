using JetBrains.Annotations;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace PaxAndromeda.Instar.Gaius;

[UsedImplicitly]
public record Warning
{
    public Snowflake GuildID { get; set; } = default!;
    public int WarnID { get; set; }
    public Snowflake UserID { get; set; } = default!;
    public string Reason { get; set; } = default!;
    [JsonConverter(typeof(UnixDateTimeConverter))]
    public DateTime WarnDate { get; set; }
    public Snowflake? PardonerID { get; set; }
    [JsonConverter(typeof(UnixDateTimeConverter))]
    public DateTime? PardonDate { get; set; }
    public Snowflake ModID { get; set; } = default!;
}