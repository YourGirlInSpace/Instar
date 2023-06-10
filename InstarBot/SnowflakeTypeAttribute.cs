namespace PaxAndromeda.Instar;

[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
public class SnowflakeTypeAttribute : Attribute
{
    public SnowflakeType Type { get; }

    public SnowflakeTypeAttribute(SnowflakeType type)
    {
        Type = type;
    }
}