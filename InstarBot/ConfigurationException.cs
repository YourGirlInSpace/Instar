using System.Diagnostics.CodeAnalysis;

namespace PaxAndromeda.Instar;

[ExcludeFromCodeCoverage]
public class ConfigurationException : Exception
{
    public ConfigurationException(string message) : base(message)
    {
    }
}