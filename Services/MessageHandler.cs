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
        _service = service;
        _logger = logger;
        _bot = bot;
    }


    public async Task<Message> OnMessage(Message msg, CancellationToken cancellationToken)
    {
        if (!cancellationToken.IsCancellationRequested)
        {
            try
            {
                string messageText = msg.Text!.Trim();

                _logger.LogWarning("Обработка команды: {messageText}", messageText);

                return messageText switch
                {
                    "/exit" => await SendStartKeyboard(msg),
                    "/start" => await SendStartKeyboard(msg),
                    "Переводчик" => await HandleTranslatorCommand(msg),
                    "Програмист" => await HandleProgrammerCommand(msg),
                    "Нейросеть" => await HandleNeuralNetworkCommand(msg),
                    _ => await SendUsageMessage(msg)
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Ошибка обработки команды: {ex.Message}");

                string text = $"Произошла ошибка при обработке команды: {ex.Message}!";

                return await _bot.SendMessage(chatId: msg.Chat.Id, text: text, cancellationToken: cancellationToken);
            }
        }

        return msg;
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


    public async Task SendProgressAnimation(Message msg)
    {
        Message sentMessage = await _bot.SendMessage(msg.Chat, "Загрузка...");

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
                _ = await _bot.EditMessageText(sentMessage.Chat, msgId, frame);
                await Task.Delay(500);
            }
        }

        _ = await _bot.EditMessageText(sentMessage.Chat, msgId, "✅ Загрузка завершена!");
    }


    #region CommandHandler

    private Task<Message> HandleProgrammerCommand(Message msg)
    {
        return ResponseHandle(msg);
    }


    private Task<Message> HandleTranslatorCommand(Message msg)
    {
        return ResponseHandle(msg);
    }


    private Task<Message> HandleNeuralNetworkCommand(Message msg)
    {
        return ResponseHandle(msg);
    }


    private async Task<Message> ResponseHandle(Message msg)
    {
        string? output = null;

        try
        {
            output = "TEST";
            //output = await _service.GetResponse(msg.Chat.Id, msg.Text!.Trim());
        }
        catch (Exception ex)
        {
            output = $"Ошибка: {ex.Message}";
        }
        finally
        {
            _logger.LogWarning(output);
        }

        return await _bot.SendMessage(msg.Chat, $"```\n{output}\n```", parseMode: ParseMode.MarkdownV2, replyMarkup: new ReplyKeyboardRemove());
    }

    #endregion


}