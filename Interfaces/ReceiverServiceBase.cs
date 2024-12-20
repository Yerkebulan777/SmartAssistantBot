using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;


namespace SmartAssistantBot.Interfaces;

/// <summary>
/// An abstract class to compose Receiver Service and Update Handler classes
/// </summary>
/// <typeparam name="TUpdateHandler">Update Handler to use in Update Receiver</typeparam>
public abstract class ReceiverServiceBase<TUpdateHandler> : IReceiverService where TUpdateHandler : IUpdateHandler
{
    private readonly ITelegramBotClient _botClient;
    private readonly IUpdateHandler _updateHandler;
    private readonly ILogger<ReceiverServiceBase<TUpdateHandler>> _logger;


    internal ReceiverServiceBase(ITelegramBotClient botClient, TUpdateHandler handler, ILogger<ReceiverServiceBase<TUpdateHandler>> logger)
    {
        _botClient = botClient;
        _updateHandler = handler;
        _logger = logger;
    }


    /// <summary>
    /// Start to service Updates with provided Update Handler class
    /// </summary>
    /// <param name="stoppingToken"></param>
    /// <returns></returns>
    public async Task ReceiveAsync(CancellationToken stoppingToken)
    {
        // ToDo: we can inject ReceiverOptions through IOptions container
        ReceiverOptions receiverOptions = new()
        {
            AllowedUpdates = [],
            DropPendingUpdates = true,
        };

        User me = await _botClient.GetMe(stoppingToken);

        _logger.LogInformation("Start receiving updates for {BotName}", me.Username ?? "SmartAssistantBot");

        // Start receiving updates
        await _botClient.ReceiveAsync(
            updateHandler: _updateHandler,
            receiverOptions: receiverOptions,
            cancellationToken: stoppingToken);
    }
}
