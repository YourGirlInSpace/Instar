using CommandLine;
using JetBrains.Annotations;

namespace PaxAndromeda.Instar;

[UsedImplicitly]
internal class CommandLineOptions
{
    [Option('c', "config-path", Required = false, HelpText = "Sets the configuration path.")]
    public string ConfigPath { get; set; } = null!;
}