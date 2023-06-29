using JetBrains.Annotations;
using Newtonsoft.Json;

namespace PaxAndromeda.Instar.Gaius;

[UsedImplicitly]
public record Caselog
{
    [UsedImplicitly] public Snowflake UserID { get; set; } = default!;
    [UsedImplicitly] public CaselogType Type { get; set; }
    [UsedImplicitly] public string Time { get; set; } = null!;
    [UsedImplicitly] public Snowflake ModID { get; set; } = default!;
    [UsedImplicitly] public string Reason { get; set; } = default!;

    [JsonConverter(typeof(UnixMillisecondDateTimeConverter)), UsedImplicitly]
    public DateTime Date { get; set; }

    [UsedImplicitly] public int CaseNum { get; set; }
}