using SmartAssistantBot.Interfaces;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;


namespace SmartAssistantBot.Services;
public class KeyboardHandler : IKeyboardHandler
{
    private const int maxButtonsPerRow = 5;
    private readonly ITelegramBotClient _botClient;
    private readonly ILogger<KeyboardHandler> _logger;


    public KeyboardHandler(ILogger<KeyboardHandler> logger, ITelegramBotClient botClient)
    {
        _botClient = botClient ?? throw new ArgumentNullException(nameof(botClient));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }


    /// <summary>
    /// Создает базовую клавиатуру с обычными кнопками.
    /// </summary>
    /// <param name="buttonTexts">Список текстов для кнопок</param>
    /// <param name="buttonsPerRow">Количество кнопок в одном ряду (по умолчанию 2)</param>
    /// <returns>Объект клавиатуры ReplyKeyboardMarkup</returns>
    /// <exception cref="ArgumentException">Возникает, если список кнопок пуст или превышен лимит кнопок в ряду</exception>
    public ReplyKeyboardMarkup CreateBasicKeyboard(List<string> buttonTexts, int buttonsPerRow = 3)
    {
        if (buttonTexts == null || !buttonTexts.Any())
        {
            throw new ArgumentException("Список кнопок не может быть пустым", nameof(buttonTexts));
        }

        if (buttonsPerRow is > maxButtonsPerRow or < 1)
        {
            throw new ArgumentException($"Количество кнопок в ряду должно быть от 1 до {maxButtonsPerRow}", nameof(buttonsPerRow));
        }

        List<List<KeyboardButton>> buttons = new();
        List<KeyboardButton> currentRow = new();

        foreach (string? text in buttonTexts.Where(txt => !string.IsNullOrEmpty(txt)))
        {
            if (currentRow.Count >= buttonsPerRow)
            {
                buttons.Add(currentRow);
                currentRow = [];
            }

            currentRow.Add(new KeyboardButton(text));
        }

        if (currentRow.Any())
        {
            buttons.Add(currentRow);
        }

        return new ReplyKeyboardMarkup(buttons) { ResizeKeyboard = true };
    }


    /// <summary>
    /// Создает инлайн-клавиатуру с кнопками обратного вызова.
    /// </summary>
    /// <param name="buttonTextAndCallbackData">Словарь с текстами кнопок и их callback-данными</param>
    /// <param name="buttonsPerRow">Количество кнопок в одном ряду (по умолчанию 2)</param>
    /// <returns>Объект клавиатуры InlineKeyboardMarkup</returns>
    /// <exception cref="ArgumentException">Возникает, если словарь пуст или превышен лимит кнопок в ряду</exception>
    public InlineKeyboardMarkup CreateInlineKeyboard(Dictionary<string, string> buttonTextAndCallbackData, int buttonsPerRow = 2)
    {
        if (buttonTextAndCallbackData == null || !buttonTextAndCallbackData.Any())
        {
            throw new ArgumentException("Словарь кнопок не может быть пустым", nameof(buttonTextAndCallbackData));
        }

        if (buttonsPerRow is > maxButtonsPerRow or < 1)
        {
            throw new ArgumentException($"Количество кнопок в ряду должно быть от 1 до {maxButtonsPerRow}", nameof(buttonsPerRow));
        }

        List<List<InlineKeyboardButton>> buttons = new();
        List<InlineKeyboardButton> currentRow = new();

        foreach ((string text, string callbackData) in buttonTextAndCallbackData)
        {
            if (currentRow.Count >= buttonsPerRow)
            {
                buttons.Add(currentRow);
                currentRow = [];
            }

            if (!string.IsNullOrEmpty(text) && !string.IsNullOrEmpty(callbackData))
            {
                currentRow.Add(InlineKeyboardButton.WithCallbackData(text, callbackData));
            }
        }

        if (currentRow.Any())
        {
            buttons.Add(currentRow);
        }

        return new InlineKeyboardMarkup(buttons);
    }


    /// <summary>
    /// Создает специальную клавиатуру с кнопками запроса контакта и/или локации.
    /// </summary>
    /// <param name="requestContact">Флаг для добавления кнопки запроса контакта</param>
    /// <param name="requestLocation">Флаг для добавления кнопки запроса локации</param>
    /// <returns>Объект клавиатуры ReplyKeyboardMarkup</returns>
    public ReplyKeyboardMarkup CreateSpecialKeyboard(bool requestContact = false, bool requestLocation = false)
    {
        List<KeyboardButton> buttons = new();

        if (requestContact)
        {
            buttons.Add(KeyboardButton.WithRequestContact("Поделиться контактом"));
        }

        if (requestLocation)
        {
            buttons.Add(KeyboardButton.WithRequestLocation("Поделиться локацией"));
        }

        return new ReplyKeyboardMarkup(new[] { buttons }) { ResizeKeyboard = true };
    }


    /// <summary>
    /// Удаляет текущую клавиатуру и отправляет сообщение пользователю.
    /// </summary>
    /// <param name="chatId">ID чата</param>
    /// <param name="message">Сообщение для отправки (по умолчанию "Клавиатура удалена")</param>
    /// <returns>Task</returns>
    public async Task RemoveKeyboard(long chatId)
    {
        _ = await _botClient.SendMessage(chatId: chatId, text: string.Empty, replyMarkup: new ReplyKeyboardRemove());
    }


    public async Task HandleCallbackQuery(CallbackQuery callbackQuery)
    {
        try
        {
            _logger.LogDebug($"Обработка callback запроса: {callbackQuery.Data}");

            await _botClient.AnswerCallbackQuery(callbackQuery.Id, "Получено!");

            if (callbackQuery.Message != null)
            {
                _ = await _botClient.SendMessage(
                    chatId: callbackQuery.Message.Chat.Id,
                    text: $"Вы выбрали: {callbackQuery.Data}");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка обработки callback запроса");
        }
    }


    public Task HandleInlineQuery(InlineQuery inlineQuery)
    {
        throw new NotImplementedException();
    }


}