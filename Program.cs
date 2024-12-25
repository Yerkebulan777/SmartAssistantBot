using Microsoft.Extensions.Options;
using Serilog;
using Serilog.Events;
using Serilog.Sinks.SystemConsole.Themes;
using SmartAssistantBot.Core;
using SmartAssistantBot.Interfaces;
using SmartAssistantBot.Models;
using SmartAssistantBot.Services;
using Telegram.Bot;


IHost host = Host.CreateDefaultBuilder(args)

    .UseSerilog((context, configuration) => configuration
        .MinimumLevel.Override("System", LogEventLevel.Warning)
        .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
        .WriteTo.Async(a => a.Console(theme: AnsiConsoleTheme.Code))
        .WriteTo.Async(a => a.File(
            path: @"C:\ProgramData\Logs\TelegramBot-.txt",
            restrictedToMinimumLevel: LogEventLevel.Debug,
            rollingInterval: RollingInterval.Day,
            retainedFileCountLimit: 3,
            shared: true))
        .Enrich.FromLogContext()
        .Enrich.WithThreadId()
        .WriteTo.Debug())

    .ConfigureServices((context, services) =>
    {
        // Register Bot configuration
        services.Configure<BotConfiguration>(context.Configuration.GetSection("BotConfiguration"));

        services.AddHttpClient("telegram_bot_client").RemoveAllLoggers()

            .AddTypedClient<ITelegramBotClient>((httpClient, sp) =>
            {
                BotConfiguration? botConfiguration = sp.GetService<IOptions<BotConfiguration>>()?.Value;
                ArgumentNullException.ThrowIfNull(botConfiguration);
                TelegramBotClientOptions options = new(botConfiguration.BotToken);
                return new TelegramBotClient(options, httpClient);
            });

        // Handlers
        services.AddScoped<IKeyboardHandler, KeyboardHandler>();
        services.AddScoped<IMessageHandler, MessageHandler>();

        // AI services
        services.AddScoped<IAiService, TranslatorAssistant>();
        services.AddScoped<IAiService, GeneralAssistant>();
        services.AddScoped<IAiService, CodingAssistant>();

        // TelegramBot services
        services.AddScoped<UpdateHandler>();
        services.AddScoped<ReceiverService>();
        services.AddHostedService<PollingService>();

    })
    .Build();

await host.RunAsync();