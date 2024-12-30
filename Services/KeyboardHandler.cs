using SmartAssistantBot.Interfaces;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.InlineQueryResults;
using Telegram.Bot.Types.ReplyMarkups;


namespace SmartAssistantBot.Services;

public class KeyboardHandler(ITelegramBotClient bot, ILogger<KeyboardHandler> logger) : IKeyboardHandler
{
    private readonly ITelegramBotClient _bot = bot ?? throw new ArgumentNullException(nameof(bot));
    private readonly ILogger<KeyboardHandler> _logger = logger ?? throw new ArgumentNullException(nameof(logger));


    public async Task<Message> SendSingleRowReplyKeyboard(Message msg, string baseText, List<string> buttonTexts)
    {
        KeyboardButton[] buttons = buttonTexts
            .Where(text => !string.IsNullOrEmpty(text))
            .Select(text => new KeyboardButton(text))
            .ToArray();

        ReplyKeyboardMarkup replyMarkup = new ReplyKeyboardMarkup(true).AddNewRow(buttons);

        return await _bot.SendMessage(msg.Chat, text: baseText, replyMarkup: replyMarkup);
    }


    public async Task<Message> SendInlineColumnKeyboard(Message msg, string baseText, Dictionary<string, string> textAndCallbackData)
    {
        InlineKeyboardMarkup inlineMarkup = new();

        foreach ((string btnTxt, string callbackData) in textAndCallbackData)
        {
            if (!string.IsNullOrEmpty(btnTxt) && !string.IsNullOrEmpty(callbackData))
            {
                _ = inlineMarkup.AddNewRow([InlineKeyboardButton.WithCallbackData(btnTxt, callbackData)]);
            }
        }

        return await _bot.SendMessage(msg.Chat, baseText, replyMarkup: inlineMarkup);
    }


    public async Task OnCallbackQuery(CallbackQuery callbackQuery)
    {
        _logger.LogDebug($"Обработка callback запроса: {callbackQuery.Data}");

        await _bot.AnswerCallbackQuery(callbackQuery.Id, $"Выбрано: {callbackQuery.Data}");

        Chat? chat = callbackQuery.Message?.Chat;

        switch (callbackQuery.Data) 
        {
            case "settings_notifications":
                await _bot.SendMessage(chat!, "Настройки уведомлений:");
                break;

            case "settings_language":
                await _bot.SendMessage(chat!, "Выберите язык:");
                break;

            case "settings_general":
                await _bot.SendMessage(chat!, "Общие настройки:");
                break;

            default:
                await bot.SendMessage(callbackQuery.Message!.Chat, $"Получено {callbackQuery.Data}");
                break;

        }
    }


    /// <summary>
    /// Пример для тестирования
    /// </summary>
    public async Task OnInlineQuery(InlineQuery inlineQuery)
    {
        InlineQueryResult[] results = [
            new InlineQueryResultArticle(
            id: "1",
            title: "Расписание на сегодня",
            new InputTextMessageContent("📅 Расписание на сегодня:\n9:00 - Встреча команды\n13:00 - Обед\n15:00 - Презентация")
        ),
        new InlineQueryResultArticle(
            id: "2",
            title: "Контакты поддержки",
            new InputTextMessageContent("📞 Служба поддержки:\nТелефон: +7 (999) 123-45-67\nEmail: support@example.com")
        ),
        new InlineQueryResultArticle(
            id: "3",
            title: "Часто задаваемые вопросы",
            new InputTextMessageContent("❓ Популярные вопросы:\n1. Как начать работу?\n2. Где найти документацию?\n3. Как связаться с менеджером?")
        )
        ];

        await bot.AnswerInlineQuery(inlineQuery.Id, results);
    }



}