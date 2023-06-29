namespace PaxAndromeda.Instar.Metrics;

[AttributeUsage(AttributeTargets.Field)]
public sealed class MetricDimensionAttribute : Attribute
{
    public string Name { get; }
    public string Value { get; }

    public MetricDimensionAttribute(string name, string value)
    {
        Name = name;
        Value = value;
    }
}