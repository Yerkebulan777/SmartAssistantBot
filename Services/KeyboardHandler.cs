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
                inlineMarkup.AddNewRow([InlineKeyboardButton.WithCallbackData(btnTxt, callbackData)]);
            }
        }

        return await _bot.SendMessage(msg.Chat, baseText, replyMarkup: inlineMarkup);
    }


    public async Task OnCallbackQuery(CallbackQuery callbackQuery)
    {
        _logger.LogDebug($"Обработка callback запроса: {callbackQuery.Data}");

        await _bot.AnswerCallbackQuery(callbackQuery.Id, $"Получено {callbackQuery.Data}");

        await bot.SendMessage(callbackQuery.Message!.Chat, $"Получено {callbackQuery.Data}");
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