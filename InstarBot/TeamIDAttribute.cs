namespace PaxAndromeda.Instar;

[AttributeUsage(AttributeTargets.Field, AllowMultiple = true)]
public class TeamIDAttribute : Attribute
{
    public ulong ID { get; set; }

    public TeamIDAttribute(ulong id)
    {
        ID = id;
    }
}