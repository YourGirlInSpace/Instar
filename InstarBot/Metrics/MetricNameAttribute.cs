namespace PaxAndromeda.Instar.Metrics;

[AttributeUsage(AttributeTargets.Field)]
public sealed class MetricNameAttribute : Attribute
{
    public string Name { get; }

    public MetricNameAttribute(string name)
    {
        Name = name;
    }
}