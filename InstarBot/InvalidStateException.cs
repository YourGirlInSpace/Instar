using System.Diagnostics.CodeAnalysis;

namespace PaxAndromeda.Instar;

[ExcludeFromCodeCoverage]
public sealed class InvalidStateException : Exception
{
    public InvalidStateException(string message) : base(message)
    {
    }
}