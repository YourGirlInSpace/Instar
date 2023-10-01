using CommandLine;
using JetBrains.Annotations;

namespace PaxAndromeda.Instar;

[UsedImplicitly]
public class CommandLineOptions
{
    [Option('c', "config-path", Required = false, Default = "Config/Instar.conf.json", HelpText = "Sets the configuration path.")]
    [UsedImplicitly]
    public string? ConfigPath { get; set; }
}