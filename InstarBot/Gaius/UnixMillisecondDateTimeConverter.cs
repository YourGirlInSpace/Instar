using System.Diagnostics.CodeAnalysis;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace PaxAndromeda.Instar.Gaius;

[SuppressMessage("ReSharper", "SwitchStatementHandlesSomeKnownEnumValuesWithDefault")]
public class UnixMillisecondDateTimeConverter : DateTimeConverterBase
{
    private static readonly DateTime UnixEpoch = new(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
    
    public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer)
    {
        throw new NotImplementedException();
    }

    public override object ReadJson(JsonReader reader, Type objectType, object? existingValue, JsonSerializer serializer)
    {
        long milliseconds;

        switch (reader.TokenType)
        {
            case JsonToken.Integer:
                milliseconds = (long)reader.Value!;
                break;
            case JsonToken.String:
            {
                if (!long.TryParse((string)reader.Value!, out milliseconds))
                {
                    throw new FormatException("Failed to deserialize datetime.");
                }

                break;
            }
            default:
                throw new FormatException($"Unexpected token parsing date. Expected Integer or String, got {reader.TokenType}");
        }
        
        return UnixEpoch.AddMilliseconds(milliseconds);
    }
}