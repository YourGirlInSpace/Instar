using Discord;
using Discord.Interactions;
using Microsoft.Extensions.Configuration;
using PaxAndromeda.Instar.Preconditions;

namespace PaxAndromeda.Instar.Commands;

public abstract class RandomPhraseCommand : BaseCommand
{
    private string[] Phrases { get; }
    private string[] NoMentionPhrases { get; }

    protected RandomPhraseCommand(IConfiguration config, string command)
    {
        var phrases = config.GetSection("FunCommands").GetSection(command).GetSection("Phrases").Get<string[]>();
        var noMentionPhrases = config.GetSection("FunCommands").GetSection(command).GetSection("Phrases").Get<string[]>();

        Phrases = phrases ?? throw new ConfigurationException($"Phrases for {command} not found!");
        NoMentionPhrases = noMentionPhrases ?? throw new ConfigurationException($"noMentionPhrases for {command} not found!");
    }

    protected string GetRandomPhrase()
    {
        return Phrases[Random.Shared.Next(0, Phrases.Length)];
    }

    protected string GetRandomNoMentionPhrase()
    {
        return NoMentionPhrases[Random.Shared.Next(0, NoMentionPhrases.Length)];
    }

    [RequireStaffMember]
    // Stupid way to hide this command for unauthorized personnel
    [DefaultMemberPermissions(GuildPermission.MuteMembers)]
    public abstract Task DoCommand(IUser user);
}