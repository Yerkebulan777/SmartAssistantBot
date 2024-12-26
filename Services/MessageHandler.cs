using SmartAssistantBot.Interfaces;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;


namespace SmartAssistantBot.Services;
public class MessageHandler : IMessageHandler
{
    private readonly ILogger<MessageHandler> _logger;
    private readonly IKeyboardHandler _keyboardHandler;
    private readonly ITelegramBotClient _bot;
    private readonly IAiService _service;


    public MessageHandler(ITelegramBotClient bot, ILogger<MessageHandler> logger, IKeyboardHandler handler, IAiService service)
    {
        _keyboardHandler = handler;
        _bot = bot;
        _service = service;
        _logger = logger;
    }


    public async Task<Message> OnMessage(Message message, CancellationToken cancellationToken)
    {
        if (!cancellationToken.IsCancellationRequested)
        {
            try
            {
                string messageText = message.Text!.Trim();
                _logger.LogDebug($"Обработка команды: {messageText}");

                return messageText switch
                {
                    "/exit" => await SendStartKeyboard(message),
                    "/start" => await SendStartKeyboard(message),
                    "Переводчик" => await HandleTranslatorCommand(message),
                    "Програмист" => await HandleProgrammerCommand(message),
                    "Нейросеть" => await HandleNeuralNetworkCommand(message),
                    _ => await SendUsageMessage(message)
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Ошибка обработки команды: {ex.Message}");

                string text = $"Произошла ошибка при обработке команды: {ex.Message}!";

                return await _bot.SendMessage(chatId: message.Chat.Id, text: text, cancellationToken: cancellationToken);
            }
        }

        return message;
    }


    private async Task<Message> SendStartKeyboard(Message msg)
    {
        string text = "Добро пожаловать! Я AI бот!\nВыберите команду:";

        List<string> buttons = ["Переводчик", "Програмист", "Нейросеть"];

        return await _keyboardHandler.SendSingleRowReplyKeyboard(msg, text, buttons);
    }


    private async Task<Message> SendUsageMessage(Message msg)
    {
        const string usage = """
            <b><u>< Бот меню ></u></b>:
            /start - Начать работу с ботом
            /exit - Выход из бота
            """;

        return await _bot.SendMessage(msg.Chat, usage, parseMode: ParseMode.Html, replyMarkup: new ReplyKeyboardRemove());
    }


    public async Task SendProgressAnimation(Message message)
    {
        Message sentMessage = await _bot.SendMessage(message.Chat, "Загрузка...");

        string[] frames = {
            "⏳ Загрузка   ",
            "⌛ Загрузка.  ",
            "⏳ Загрузка.. ",
            "⌛ Загрузка..."
        };

        int msgId = sentMessage.Id;

        for (int i = 0; i < 5; i++)
        {
            foreach (string frame in frames)
            {
                await _bot.EditMessageText(sentMessage.Chat, msgId, frame);
                await Task.Delay(500);
            }
        }

        await _bot.EditMessageText(sentMessage.Chat, msgId, "✅ Загрузка завершена!");
    }


    #region CommandHandler

    private Task<Message> HandleProgrammerCommand(Message message)
    {
        return ResponseHandle(message);
    }


    private Task<Message> HandleNeuralNetworkCommand(Message message)
    {
        return ResponseHandle(message);
    }


    private Task<Message> HandleTranslatorCommand(Message message)
    {
        return ResponseHandle(message);
    }


    private async Task<Message> ResponseHandle(Message message)
    {
        long chatId = message.Chat.Id;

        try
        {
            string result = await _service.GetResponse(chatId, message.Text!.Trim());
            return await _bot.SendMessage(chatId: chatId, text: $"```\n{result}\n```", parseMode: ParseMode.MarkdownV2);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Ошибка {ex.Message}");
            return await _bot.SendMessage(chatId: chatId, text: $"Ошибка {ex.Message}");
        }
    }

    #endregion


}