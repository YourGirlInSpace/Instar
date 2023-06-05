using Discord;
using Discord.WebSocket;

namespace PaxAndromeda.Instar.Commands;

public interface IContextCommand
{
    string Name { get; }
    Task HandleCommand(SocketMessageCommand arg);
    MessageCommandProperties CreateCommand();
}