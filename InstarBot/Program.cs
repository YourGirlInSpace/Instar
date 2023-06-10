using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Schema;
using PaxAndromeda.Instar.Commands;
using PaxAndromeda.Instar.Services;
using Serilog;
using Serilog.Events;

namespace PaxAndromeda.Instar;

internal static class Program
{
    private static CancellationTokenSource _cts = null!;
    private static IServiceProvider _services = null!;

    public static async Task Main()
    {
        InitializeLogger();
        AppDomain.CurrentDomain.UnhandledException += CurrentDomainOnUnhandledException;

        #if DEBUG
        const string configPath = "Config/Instar.debug.conf.json";
        #else
        const string configPath = "Config/Instar.conf.json";
        #endif
        
        // Initial check:  Is the configuration valid?
        try
        {
            ValidateConfiguration(configPath);
        }
        catch (Exception ex)
        {
            Log.Fatal(ex, "Malformed configuration!  Aborting!");
            return;
        }

        IConfiguration config = new ConfigurationBuilder()
            .AddJsonFile(configPath)
            .Build();
        
        Console.CancelKeyPress += StopSystem;
        await RunAsync(config);

        while (!_cts.IsCancellationRequested) await Task.Delay(100);
    }
    private static async void StopSystem(object? sender, ConsoleCancelEventArgs e)
    {
        await _services.GetRequiredService<DiscordService>().Stop();
        _cts.Cancel();
    }

    private static void ValidateConfiguration(string configPath)
    {
        var schemaData = File.ReadAllText(Path.Combine("Config", "Instar.conf.schema.json"));
        var configData = File.ReadAllText(configPath);
        
        var schema = JSchema.Parse(schemaData);

        var jObject = JObject.Parse(configData);
        jObject.Validate(schema);
    }


    private static async Task RunAsync(IConfiguration config)
    {
        _cts = new CancellationTokenSource();
        _services = ConfigureServices(config);

        var discordService = _services.GetRequiredService<DiscordService>();
        await discordService.Start(_services);
    }

    private static void InitializeLogger()
    {
#if TRACE
        const LogEventLevel minLevel = LogEventLevel.Debug;
#elif DEBUG
            const LogEventLevel minLevel = LogEventLevel.Verbose;
#else
            const LogEventLevel minLevel = LogEventLevel.Information;
#endif

        Log.Logger = new LoggerConfiguration()
            .Enrich.FromLogContext()
            .MinimumLevel.Is(minLevel)
            .WriteTo.Console()
            .CreateLogger();
    }

    private static void CurrentDomainOnUnhandledException(object sender, UnhandledExceptionEventArgs e)
    {
        Log.Fatal(e.ExceptionObject as Exception, "FATAL: Unhandled exception caught.");
    }

    private static IServiceProvider ConfigureServices(IConfiguration config)
    {
        var services = new ServiceCollection();

        services.AddSingleton(config);
        services.AddSingleton<TeamService>();
        services.AddTransient<PingCommand>();
        services.AddTransient<SetBirthdayCommand>();
        services.AddSingleton<PageCommand>();
        services.AddTransient<IContextCommand, ReportUserCommand>();
        services.AddSingleton<DiscordService>();

        return services.BuildServiceProvider();
    }
}