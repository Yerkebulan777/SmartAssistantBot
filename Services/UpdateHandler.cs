using SmartAssistantBot.Interfaces;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;


namespace SmartAssistantBot.Services;
public class UpdateHandler : IUpdateHandler
{
    private const int delayMilliseconds = 1500;
    private readonly ILogger<UpdateHandler> _logger;
    private readonly IMessageHandler _messageHandler;
    private readonly IKeyboardHandler _keyboardHandler;


    public UpdateHandler(ILogger<UpdateHandler> logger, IMessageHandler messageHandler, IKeyboardHandler keyboardHandler)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _messageHandler = messageHandler ?? throw new ArgumentNullException(nameof(messageHandler));
        _keyboardHandler = keyboardHandler ?? throw new ArgumentNullException(nameof(keyboardHandler));
    }


    public async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        try
        {
            Task handler = update.Type switch
            {
                UpdateType.Message when update.Message?.Text != null =>
                    HandleMessageAsync(update.Message, cancellationToken),

                UpdateType.CallbackQuery =>
                    HandleCallbackQueryAsync(update.CallbackQuery!, cancellationToken),

                UpdateType.InlineQuery =>
                    HandleInlineQueryAsync(update.InlineQuery!, cancellationToken),

                _ => UnhandledUpdateTypeAsync(update.Type)
            };

            await handler;
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "Error while handling update");
            throw;
        }
    }


    private async Task HandleMessageAsync(Message message, CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            $"Received message. Type: {message.Type}, " +
            $"From: {message.From?.Username ?? "Unknown"}, " +
            $"Text: {message.Text ?? "Empty"}");

        try
        {
            _ = await _messageHandler.HandleCommand(message, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing message");
            throw;
        }
    }


    private async Task HandleCallbackQueryAsync(CallbackQuery callbackQuery, CancellationToken cancellationToken)
    {
        string? user = callbackQuery.From.Username;

        _logger.LogInformation($"Получен callback-запрос от пользователя: {user} : {callbackQuery.Data}");

        // Проверяем, не была ли запрошена отмена операции
        if (!cancellationToken.IsCancellationRequested)
        {
            try
            {
                // Передаем callback-запрос обработчику сообщений
                await _keyboardHandler.HandleCallbackQuery(callbackQuery);
            }
            catch (Exception ex)
            {
                // Логируем ошибку, если она возникла при обработке
                _logger.LogError(ex, "Ошибка при обработке callback-запроса");
                throw;
            }
        }
    }


    private async Task HandleInlineQueryAsync(InlineQuery inlineQuery, CancellationToken cancellationToken)
    {
        string? user = inlineQuery.From.Username;

        _logger.LogInformation($"Received inline query. From: {user}, Query: {inlineQuery.Query}");

        if (!cancellationToken.IsCancellationRequested)
        {
            try
            {
                await _keyboardHandler.HandleInlineQuery(inlineQuery);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing inline query");
                throw;
            }
        }
    }


    private Task UnhandledUpdateTypeAsync(UpdateType updateType)
    {
        _logger.LogWarning($"Unhandled update type: {updateType}");
        return Task.CompletedTask;
    }


    #region HandleErrorRegion

    public Task HandleErrorAsync(ITelegramBotClient botClient, Exception exception, HandleErrorSource source, CancellationToken cancellationToken)
    {
        string errorMessage = exception switch
        {
            ApiRequestException apiRequestException =>
                $"Telegram API Error:[{apiRequestException.ErrorCode}] {apiRequestException.Message}",

            TimeoutException =>
                "Timeout exceeded. The operation was canceled.",

            TaskCanceledException =>
                "Task was cancelled. Operation aborted.",

            _ => exception.ToString()
        };

        _logger.LogError(exception, $"Source: {source} Error: {errorMessage}");

        // Обработка специфических ошибок API
        if (exception is ApiRequestException apiException)
        {
            HandleApiException(apiException, cancellationToken);
        }

        // Обработка сетевых ошибок
        else if (exception is RequestException)
        {
            HandleRequestException(cancellationToken);
        }

        return Task.CompletedTask;
    }


    public Task HandlePollingErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
    {
        string errorMessage = exception switch
        {
            ApiRequestException apiRequestException =>
                $"Ошибка Telegram API: [{apiRequestException.ErrorCode}] {apiRequestException.Message}",

            TimeoutException =>
                "Превышено время ожидания. Операция была отменена.",

            TaskCanceledException =>
                "Задача была отменена. Операция прервана.",

            _ => exception.ToString()
        };

        _logger.LogError(exception, $"Ошибка получения обновлений: {errorMessage}");

        // Обработка специфических исключений API Telegram
        if (exception is ApiRequestException apiException)
        {
            HandleApiException(apiException, cancellationToken);
        }

        // Обработка сетевых ошибок
        else if (exception is RequestException)
        {
            HandleRequestException(cancellationToken);
        }

        return Task.CompletedTask;
    }


    private void HandleApiException(ApiRequestException exception, CancellationToken cancellationToken)
    {
        switch (exception.ErrorCode)
        {
            case 429: // Слишком много запросов
                _logger.LogError("Превышен лимит запросов. Добавляем задержку.");
                Task.Delay(5000, cancellationToken).Wait(cancellationToken);
                break;

            case 403: // Доступ запрещен
                _logger.LogError("Бот был заблокирован пользователем");
                break;

            case 400: // Неверный запрос
                _logger.LogError("Ошибка в запросе");
                break;

            default:
                _logger.LogError($"Неожиданная ошибка: Код {exception.ErrorCode}, Сообщение: {exception.Message}");
                break;
        }
    }


    private void HandleRequestException(CancellationToken cancellationToken)
    {
        _logger.LogWarning("Network error occurred. Implementing delay.");
        Task.Delay(delayMilliseconds, cancellationToken).Wait(cancellationToken);
    }

    #endregion


}