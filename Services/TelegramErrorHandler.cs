using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Polling;

namespace SmartAssistantBot.Services;

public class TelegramErrorHandler
{
    private readonly ILogger<TelegramErrorHandler> _logger;

    public TelegramErrorHandler(ILogger<TelegramErrorHandler> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }





}