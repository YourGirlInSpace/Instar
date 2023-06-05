using JetBrains.Annotations;

namespace PaxAndromeda.Instar;

internal abstract class Team
{
    public string Name { get; set; } = null!;

    // ReSharper disable once InconsistentNaming
    public ulong ID { get; [UsedImplicitly] set; }

    // ReSharper disable once IdentifierTypo
    public ulong Teamleader { get; [UsedImplicitly] set; }
    public uint Color { get; [UsedImplicitly] set; }
    public int Priority { get; [UsedImplicitly] set; }
}