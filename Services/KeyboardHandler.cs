using SmartAssistantBot.Interfaces;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.InlineQueryResults;
using Telegram.Bot.Types.ReplyMarkups;

namespace SmartAssistantBot.Services;
public class KeyboardHandler(ITelegramBotClient bot, ILogger<KeyboardHandler> logger) : IKeyboardHandler
{
    private const int maxButtonsPerRow = 5;
    private readonly ITelegramBotClient _bot = bot ?? throw new ArgumentNullException(nameof(bot));
    private readonly ILogger<KeyboardHandler> _logger = logger ?? throw new ArgumentNullException(nameof(logger));


    /// <summary>
    /// Отправляет клавиатуру с одним рядом кнопок.
    /// </summary>
    /// <param name="msg">Сообщение, на основе которого будет отправлена клавиатура</param>
    /// <param name="buttonTexts">Список текстов для кнопок одного ряда</param>
    /// <returns>Отправленное сообщение с клавиатурой</returns>
    public async Task<Message> SendSingleRowKeyboard(Message msg, string text, List<string> buttonTexts)
    {
        KeyboardButton[] buttons = buttonTexts
            .Where(text => !string.IsNullOrEmpty(text))
            .Select(text => new KeyboardButton(text))
            .ToArray();

        ReplyKeyboardMarkup replyMarkup = new ReplyKeyboardMarkup(true).AddNewRow(buttons);

        return await _bot.SendMessage(msg.Chat, text: text, replyMarkup: replyMarkup);
    }


    /// <summary>
    /// Отправляет инлайн клавиатуру.
    /// </summary>
    public async Task<Message> SendInlineKeyboard(Message msg, Dictionary<string, string> buttonTextAndCallbackData, int buttonsPerRow = 2)
    {
        if (buttonTextAndCallbackData == null || !buttonTextAndCallbackData.Any())
        {
            throw new ArgumentException("Словарь кнопок не может быть пустым", nameof(buttonTextAndCallbackData));
        }

        if (buttonsPerRow is > maxButtonsPerRow or < 1)
        {
            throw new ArgumentException($"Количество кнопок в ряду должно быть от 1 до {maxButtonsPerRow}", nameof(buttonsPerRow));
        }

        InlineKeyboardMarkup inlineMarkup = new();
        List<(string text, string data)> currentRow = [];

        foreach ((string text, string callbackData) in buttonTextAndCallbackData)
        {
            if (!string.IsNullOrEmpty(text) && !string.IsNullOrEmpty(callbackData))
            {
                currentRow.Add((text, callbackData));
                if (currentRow.Count >= buttonsPerRow)
                {
                    _ = inlineMarkup.AddNewRow(currentRow.Select(x =>
                        InlineKeyboardButton.WithCallbackData(x.text, x.data)).ToArray());
                    currentRow.Clear();
                }
            }
        }

        if (currentRow.Any())
        {
            _ = inlineMarkup.AddNewRow(currentRow.Select(x =>
                InlineKeyboardButton.WithCallbackData(x.text, x.data)).ToArray());
        }

        return await _bot.SendMessage(msg.Chat, "Inline buttons:", replyMarkup: inlineMarkup);
    }


    public async Task OnCallbackQuery(CallbackQuery callbackQuery)
    {
        try
        {
            _logger.LogDebug($"Обработка callback запроса: {callbackQuery.Data}");

            await _bot.AnswerCallbackQuery(
                callbackQueryId: callbackQuery.Id,
                text: $"Received {callbackQuery.Data}");

            if (callbackQuery.Message != null)
            {
                _ = await _bot.SendMessage(
                    chatId: callbackQuery.Message.Chat.Id,
                    text: $"Received {callbackQuery.Data}");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка обработки callback запроса");
        }
    }


    public async Task OnInlineQuery(InlineQuery inlineQuery)
    {
        _logger.LogInformation("Received inline query from: {InlineQueryFromId}", inlineQuery.From.Id);

        InlineQueryResult[] results = [ // displayed result
            new InlineQueryResultArticle("1", "Telegram.Bot", new InputTextMessageContent("hello")),
            new InlineQueryResultArticle("2", "is the best", new InputTextMessageContent("world"))
        ];

        await bot.AnswerInlineQuery(inlineQuery.Id, results, cacheTime: 0, isPersonal: true);
    }


}