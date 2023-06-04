﻿using System;
using Discord;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using PaxAndromeda.Instar.Commands;
using PaxAndromeda.Instar.Services;
using Serilog;
using Serilog.Events;

namespace PaxAndromeda.Instar
{
    internal class Program
    {
        private static CancellationTokenSource _cts;
        private static IServiceProvider _services;

        public static async Task Main()
        {
            AppDomain.CurrentDomain.UnhandledException += CurrentDomainOnUnhandledException;

            IConfiguration config = new ConfigurationBuilder()
#if DEBUG
                .AddJsonFile("Config/Instar.debug.conf.json")
#else
                .AddJsonFile("Config/Instar.conf.json")
#endif
                .Build();


            Console.CancelKeyPress += StopSystem;
            await RunAsync(config);

            while (!_cts.IsCancellationRequested)
            {
                await Task.Delay(100);
            }
        }

        private static async void StopSystem(object? sender, ConsoleCancelEventArgs e)
        {
            await _services.GetRequiredService<DiscordService>().Stop();
            _cts.Cancel();
        }

        private static async Task RunAsync(IConfiguration config)
        {
            _cts = new CancellationTokenSource();
            _services = ConfigureServices(config);
            InitializeLogger(_services);

            DiscordService discordService = _services.GetRequiredService<DiscordService>();
            await discordService.Start(_services);
        }

        private static void InitializeLogger(IServiceProvider services)
        {
            IConfiguration config = services.GetRequiredService<IConfiguration>();

#if TRACE
            const LogEventLevel minLevel = LogEventLevel.Verbose;
#elif DEBUG
            const LogEventLevel minLevel = LogEventLevel.Debug;
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
            => Log.Fatal(e.ExceptionObject as Exception, "FATAL: Unhandled exception caught.");

        private static IServiceProvider ConfigureServices(IConfiguration config)
        {
            var services = new ServiceCollection();

            services.AddSingleton(config);
            services.AddTransient<PingCommand>();
            services.AddTransient<SetBirthdayCommand>();
            services.AddSingleton<PageCommand>();
            services.AddTransient<IContextCommand, ReportUserCommand>();
            services.AddSingleton<DiscordService>();

            return services.BuildServiceProvider();
        }
    }
}