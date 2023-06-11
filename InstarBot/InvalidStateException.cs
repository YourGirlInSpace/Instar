using System.Diagnostics.CodeAnalysis;

namespace PaxAndromeda.Instar;

[ExcludeFromCodeCoverage]
public class InvalidStateException : Exception
{
    public InvalidStateException(string message) : base(message)
    {
    }
}