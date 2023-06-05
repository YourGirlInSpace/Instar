using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using Serilog.Events;
using System.Windows.Input;
using PaxAndromeda.Instar.Commands;

namespace PaxAndromeda.Instar.Services;

public class DiscordService
{
    private readonly IServiceProvider _provider;
    private DiscordSocketClient socketClient;
    private InteractionService interactionService;
    
    private readonly string _botToken;
    private readonly ulong _guild;
    private readonly ulong _channel;

    private readonly Dictionary<string, IContextCommand> _contextCommands;

    public DiscordService(IServiceProvider provider, IConfiguration config)
    {
        _provider = provider;

        socketClient = new DiscordSocketClient(new DiscordSocketConfig
        {
            GatewayIntents = GatewayIntents.GuildMembers | GatewayIntents.Guilds | GatewayIntents.AllUnprivileged,
            LogLevel = LogSeverity.Verbose
        });

        interactionService = new InteractionService(socketClient, new InteractionServiceConfig
        {
            LogLevel = LogSeverity.Verbose,
            ThrowOnError = true
        });

        socketClient.Log += HandleDiscordLog;
        socketClient.InteractionCreated += HandleInteraction;
        socketClient.MessageCommandExecuted += HandleMessageCommand;
        interactionService.Log += HandleDiscordLog;

        _contextCommands = provider.GetServices<IContextCommand>().ToDictionary(n => n.Name, n => n);
        
        _guild = config.GetValue("TargetGuild", 0ul);
        _channel = config.GetValue("TargetChannel", 0ul);
        _botToken = config.GetValue<string>("Token") ?? string.Empty;

        // Validate
        if (_guild == 0)
            throw new ConfigurationException("TargetGuild is not set");
        if (_channel == 0)
            throw new ConfigurationException("TargetChannel is not set");
        if (string.IsNullOrEmpty(_botToken))
            throw new ConfigurationException("Token is not set");
    }

    private async Task HandleMessageCommand(SocketMessageCommand arg)
    {
        Log.Information("Message command: {CommandName}", arg.CommandName);

        if (!_contextCommands.ContainsKey(arg.CommandName))
        {
            Log.Warning("Received message command interaction for unknown command by name {CommandName}", arg.CommandName);
            return;
        }

        await _contextCommands[arg.CommandName].HandleCommand(arg);
    }

    private async Task HandleInteraction(SocketInteraction arg)
    {
        try
        {
            // Create an execution context that matches the generic type parameter of your InteractionModuleBase<T> modules
            var ctx = new SocketInteractionContext(socketClient, arg);

            Log.Verbose("Handling interaction of type {InteractionType} with ID {InteractionID} from user {UserName} ({UserID})", ctx.Interaction.Type, ctx.Interaction.Id, ctx.User.Username, ctx.User.Id);

            await interactionService.ExecuteCommandAsync(ctx, _provider);
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

        socketClient.Ready += async () =>
        {
            Log.Information("Discord client is ready");
            await interactionService.AddModulesAsync(typeof(DiscordService).Assembly, provider);
            await interactionService.RegisterCommandsGloballyAsync();

            var props = _contextCommands.Values.Select(n => n.CreateCommand())
                .Cast<ApplicationCommandProperties>().ToArray();

            await socketClient.BulkOverwriteGlobalApplicationCommandsAsync(props);
        };

        Log.Verbose("Attempting login...");
        await socketClient.LoginAsync(TokenType.Bot, _botToken);
        Log.Verbose("Starting Discord...");
        await socketClient.StartAsync();
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
        await socketClient.StopAsync();
        await socketClient.LogoutAsync();
    }
}