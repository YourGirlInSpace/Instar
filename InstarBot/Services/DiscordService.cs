﻿using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using PaxAndromeda.Instar.Commands;
using Serilog;
using Serilog.Events;

namespace PaxAndromeda.Instar.Services;

public class DiscordService
{
    private readonly string _botToken;

    private readonly Dictionary<string, IContextCommand> _contextCommands;
    private readonly ulong _guild;
    private readonly InteractionService _interactionService;
    private readonly IServiceProvider _provider;
    private readonly DiscordSocketClient _socketClient;

    public DiscordService(IServiceProvider provider, IConfiguration config)
    {
        _provider = provider;

        _socketClient = new DiscordSocketClient(new DiscordSocketConfig
        {
            // All privileges except for GuildScheduledEvents and GuildInvites.
            // This is written to prevent warnings about these two privileges.
            GatewayIntents = GatewayIntents.AllUnprivileged
                             & ~(GatewayIntents.GuildScheduledEvents | GatewayIntents.GuildInvites),
            LogLevel = LogSeverity.Debug
        });

        _interactionService = new InteractionService(_socketClient, new InteractionServiceConfig
        {
            LogLevel = LogSeverity.Debug,
            ThrowOnError = true
        });

        _socketClient.Log += HandleDiscordLog;
        _socketClient.InteractionCreated += HandleInteraction;
        _socketClient.MessageCommandExecuted += HandleMessageCommand;
        _interactionService.Log += HandleDiscordLog;

        _contextCommands = provider.GetServices<IContextCommand>().ToDictionary(n => n.Name, n => n);

        _guild = config.GetValue("TargetGuild", 0ul);
        _botToken = config.GetValue<string>("Token") ?? string.Empty;

        // Validate
        if (_guild == 0)
            throw new ConfigurationException("TargetGuild is not set");
        if (string.IsNullOrEmpty(_botToken))
            throw new ConfigurationException("Token is not set");
    }

    private async Task HandleMessageCommand(SocketMessageCommand arg)
    {
        Log.Information("Message command: {CommandName}", arg.CommandName);

        if (!_contextCommands.ContainsKey(arg.CommandName))
        {
            Log.Warning("Received message command interaction for unknown command by name {CommandName}",
                arg.CommandName);
            return;
        }

        await _contextCommands[arg.CommandName].HandleCommand(arg);
    }

    private async Task HandleInteraction(SocketInteraction arg)
    {
        try
        {
            // Create an execution context that matches the generic type parameter of your InteractionModuleBase<T> modules
            var ctx = new SocketInteractionContext(_socketClient, arg);

            Log.Verbose(
                "Handling interaction of type {InteractionType} with ID {InteractionID} from user {UserName} ({UserID})",
                ctx.Interaction.Type, ctx.Interaction.Id, ctx.User.Username, ctx.User.Id);

            await _interactionService.ExecuteCommandAsync(ctx, _provider);
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Failed to handle interaction.");

            // If a Slash Command execution fails it is most likely that the original interaction acknowledgement will persist. It is a good idea to delete the original
            // response, or at least let the user know that something went wrong during the command execution.
            if (arg.Type == InteractionType.ApplicationCommand)
                await arg.GetOriginalResponseAsync().ContinueWith(async msg => await msg.Result.DeleteAsync());
        }
    }

    public async Task Start(IServiceProvider provider)
    {
        Log.Information("Attempting to connect to Discord...");

        _socketClient.Ready += async () =>
        {
            Log.Information("Discord client is ready");
            await _interactionService.AddModulesAsync(typeof(DiscordService).Assembly, provider);

            Log.Information("Registering commands...");
            var result = await _interactionService.RegisterCommandsToGuildAsync(_guild);

            foreach (var globalCmd in result)
                Log.Information(
                    "Registered command {CommandName} with ID {CommandID} and default permission {DefaultPerm}",
                    globalCmd.Name, globalCmd.Id, globalCmd.DefaultMemberPermissions);

            var props = _contextCommands.Values.Select(n => n.CreateCommand())
                .Cast<ApplicationCommandProperties>().ToArray();

            await _socketClient.BulkOverwriteGlobalApplicationCommandsAsync(props);
        };

        Log.Verbose("Attempting login...");
        await _socketClient.LoginAsync(TokenType.Bot, _botToken);
        Log.Verbose("Starting Discord...");
        await _socketClient.StartAsync();
    }

    private static Task HandleDiscordLog(LogMessage arg)
    {
        var severity = arg.Severity switch
        {
            LogSeverity.Critical => LogEventLevel.Fatal,
            LogSeverity.Error => LogEventLevel.Error,
            LogSeverity.Warning => LogEventLevel.Warning,
            LogSeverity.Info => LogEventLevel.Information,
            LogSeverity.Verbose => LogEventLevel.Verbose,
            LogSeverity.Debug => LogEventLevel.Debug,
            _ => LogEventLevel.Information
        };

        Log.Write(severity, arg.Exception, arg.Message);
        return Task.CompletedTask;
    }

    public async Task Stop()
    {
        Log.Information("Stopping!");
        await _socketClient.StopAsync();
        await _socketClient.LogoutAsync();
    }
}