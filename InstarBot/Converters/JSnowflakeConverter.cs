using Newtonsoft.Json;

namespace PaxAndromeda.Instar.Converters;

public sealed class JSnowflakeConverter : JsonConverter<Snowflake>
{
    public override bool CanRead => true;
    public override bool CanWrite => false;

    public override void WriteJson(JsonWriter writer, Snowflake? value, JsonSerializer serializer)
    {
    }

    public override Snowflake ReadJson(JsonReader reader, Type objectType, Snowflake? existingValue, bool hasExistingValue,
        JsonSerializer serializer)
    {
        if (reader.TokenType == JsonToken.Null)
            return null!;
        
        if (reader.Value is string snowflakeString)
            return new Snowflake(ulong.Parse(snowflakeString));
        
        if (reader.Value is not long id)
            throw new ConfigurationException("Could not decipher snowflake");

        return new Snowflake((ulong) id);
    }
}