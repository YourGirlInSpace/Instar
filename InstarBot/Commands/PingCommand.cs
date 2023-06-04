﻿using Discord;
using Discord.Interactions;
using JetBrains.Annotations;

namespace PaxAndromeda.Instar.Commands;

public class PingCommand : InteractionModuleBase<SocketInteractionContext>
{
    public string Name { get; }

    public PingCommand()
    {
        Name = "ping";
    }

    [UsedImplicitly]
    [RequireOwner]
    [DefaultMemberPermissions(GuildPermission.Administrator)]
    [SlashCommand("ping", "Provides a way to test whether Instar is online.")]
    public async Task Ping()
    {
        await RespondAsync("Pong!", ephemeral: true);
    }
}