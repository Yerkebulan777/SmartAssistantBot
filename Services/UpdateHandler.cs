using SmartAssistantBot.Interfaces;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;


namespace SmartAssistantBot.Services;
public class UpdateHandler(ILogger<UpdateHandler> logger, IMessageHandler msgHandler, IKeyboardHandler kbrHandler) : IUpdateHandler
{
    private readonly ILogger<UpdateHandler> _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    private readonly IMessageHandler _messageHandler = msgHandler ?? throw new ArgumentNullException(nameof(msgHandler));
    private readonly IKeyboardHandler _keyboardHandler = kbrHandler ?? throw new ArgumentNullException(nameof(kbrHandler));


    public async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        _logger.LogInformation("Received update: {UpdateType}", update.Type);

        try
        {
            Task handler = update.Type switch
            {
                UpdateType.Message when update.Message?.Text != null =>
                    _messageHandler.OnMessage(update.Message, cancellationToken),

                UpdateType.EditedMessage when update.EditedMessage?.Text != null =>
                    _messageHandler.OnMessage(update.EditedMessage, cancellationToken),

                UpdateType.CallbackQuery when update.CallbackQuery != null =>
                    _keyboardHandler.OnCallbackQuery(update.CallbackQuery),

                UpdateType.InlineQuery when update.InlineQuery != null =>
                    _keyboardHandler.OnInlineQuery(update.InlineQuery),

                _ => UnknownUpdateHandler(update.Type)
            };

            await handler;
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "Error while handling update");
            throw;
        }
    }


    private Task UnknownUpdateHandler(UpdateType updateType)
    {
        _logger.LogWarning($"Unhandled update type: {updateType}");

        return Task.CompletedTask;
    }


    #region HandleErrorAsync 

    public async Task HandleErrorAsync(ITelegramBotClient botClient, Exception exception, HandleErrorSource source, CancellationToken cancellationToken)
    {
        try
        {
            (string errorMessage, Task handling) = exception switch
            {
                ApiRequestException apiException => (
                    $"Telegram API Error:[{apiException.ErrorCode}] {apiException.Message}",
                    HandleApiExceptionAsync(apiException, cancellationToken)
                ),

                RequestException => (
                    "Network request error occurred",
                    HandleRequestExceptionAsync(cancellationToken)
                ),

                TimeoutException => (
                    "Timeout exceeded. The operation was canceled.",
                    Task.CompletedTask
                ),

                TaskCanceledException => (
                    "Task was cancelled. Operation aborted.",
                    Task.CompletedTask
                ),

                _ => (exception.ToString(), Task.CompletedTask)
            };

            _logger.LogError(exception, $"Source: {source} Error: {errorMessage}");
            await handling;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while handling the original exception");
        }
    }


    private async Task HandleApiExceptionAsync(ApiRequestException exception, CancellationToken cancellationToken)
    {
        string logMessage = exception.ErrorCode switch
        {
            429 => "Rate limit exceeded. Adding delay before retry.",
            403 => "Bot was blocked by user or lacks necessary permissions",
            400 => "Invalid request parameters",
            401 => "Bot token is invalid",
            404 => "Requested resource not found",
            _ => $"Unexpected API error: Code {exception.ErrorCode}"
        };

        _logger.LogError(logMessage);

        await Task.Delay(1500, cancellationToken);
    }


    private async Task HandleRequestExceptionAsync(CancellationToken cancellationToken)
    {
        _logger.LogWarning("Network error occurred. Implementing delay before retry.");
        await Task.Delay(3000, cancellationToken);
    }

    #endregion


}