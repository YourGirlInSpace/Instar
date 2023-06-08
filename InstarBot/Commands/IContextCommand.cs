using Discord;

namespace PaxAndromeda.Instar.Commands;

public interface IContextCommand
{
    string Name { get; }
    Task HandleCommand(IInstarMessageCommandInteraction arg);
    MessageCommandProperties CreateCommand();
}