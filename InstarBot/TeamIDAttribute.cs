namespace PaxAndromeda.Instar;

[AttributeUsage(AttributeTargets.Field, AllowMultiple = true)]
public class TeamIDAttribute : Attribute
{
    public TeamIDAttribute(ulong id)
    {
        ID = id;
    }

    public ulong ID { get; set; }
}