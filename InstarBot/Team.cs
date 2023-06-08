using JetBrains.Annotations;

namespace PaxAndromeda.Instar;

public class Team
{
    public string Name { get; [UsedImplicitly] set; } = null!;

    // ReSharper disable once InconsistentNaming
    public ulong ID { get; [UsedImplicitly] set; }

    // ReSharper disable once IdentifierTypo
    public ulong Teamleader { get; [UsedImplicitly] set; }
    public uint Color { get; [UsedImplicitly] set; }
    public int Priority { get; [UsedImplicitly] set; }
}