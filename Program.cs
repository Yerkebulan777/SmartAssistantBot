using Microsoft.Extensions.Options;
using Serilog;
using SmartAssistantBot.Core;
using SmartAssistantBot.Interfaces;
using SmartAssistantBot.Models;
using SmartAssistantBot.Services;
using Telegram.Bot;


const string logPath = "logs/telegramBot.txt";

IHost host = Host.CreateDefaultBuilder(args)

    .UseSerilog((context, services, configuration) => configuration
        .ReadFrom.Configuration(context.Configuration)
        .ReadFrom.Services(services)
        .WriteTo.Console()
        .WriteTo.File(logPath)
        .Enrich.FromLogContext())

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

        // HandlerServices
        services.AddScoped<IKeyboardHandler, KeyboardHandler>();

        // MessageService
        services.AddScoped<IMessageHandler, MessageHandler>();

        // AI Services
        services.AddScoped<IAiService, TranslatorAssistant>();
        services.AddScoped<IAiService, GeneralAssistant>();
        services.AddScoped<IAiService, CodingAssistant>();

        // UpdateHandlerServices
        services.AddScoped<UpdateHandler>();
        services.AddScoped<ReceiverService>();
        services.AddHostedService<PollingService>();

    })
    .Build();

await host.RunAsync();