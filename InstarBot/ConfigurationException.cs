using System.Diagnostics.CodeAnalysis;

namespace PaxAndromeda.Instar;

[ExcludeFromCodeCoverage]
public sealed class ConfigurationException : Exception
{
    public ConfigurationException(string message) : base(message)
    {
    }
}