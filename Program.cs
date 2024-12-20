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
    .MinimumLevel.Debug()
    .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
    .MinimumLevel.Override("System", LogEventLevel.Warning)
    .WriteTo.Async(a => a.Console(theme: AnsiConsoleTheme.Code))
    .WriteTo.Async(a => a.File(
        path: "logs/telegramBot-.txt",
        rollingInterval: RollingInterval.Day,
        shared: true))
    .Enrich.FromLogContext()
    .Enrich.WithThreadId())

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