using JetBrains.Annotations;
using Newtonsoft.Json;

namespace PaxAndromeda.Instar.Gaius;

[UsedImplicitly]
public record Caselog
{
    public Snowflake UserID { get; set; } = default!;
    public CaselogType Type { get; set; }
    public TimeSpan? Time { get; set; }
    public Snowflake ModID { get; set; } = default!;
    public string Reason { get; set; } = default!;
    [JsonConverter(typeof(UnixMillisecondDateTimeConverter))]
    public DateTime Date { get; set; }
    public int CaseNum { get; set; }
}