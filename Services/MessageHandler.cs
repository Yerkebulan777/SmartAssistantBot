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
    private readonly ITelegramBotClient _botClient;
    private readonly IAiService _service;


    public MessageHandler(ITelegramBotClient botClient, ILogger<MessageHandler> logger, IKeyboardHandler handler, IAiService service)
    {
        _keyboardHandler = handler;
        _botClient = botClient;
        _service = service;
        _logger = logger;
    }


    private string GetUserName(Message message)
    {
        long chatId = message.Chat.Id;
        User? user = message.From;

        if (user is null)
        {
            return $"User({chatId})";
        }

        string userName = user.Username ?? $"{user.FirstName} {user.LastName}";

        return userName.Trim();
    }


    public async Task<Message> HandleCommand(Message message, CancellationToken cancellationToken)
    {
        // Проверяем, не была ли запрошена отмена операции
        if (!cancellationToken.IsCancellationRequested)
        {
            try
            {
                string messageText = message.Text!.Trim();

                _logger.LogDebug($"Обработка команды: {messageText}");

                string userName = GetUserName(message);

                // Обрабатка команд
                switch (messageText)
                {
                    case "/help":
                        return await SendHelpMessage(message);

                    case "/start":
                        return await SendStartKeyboard(message);

                    case "/exit":
                        return await SendStartKeyboard(message);

                    case "Переводчик":
                        return await HandleTranslatorCommand(message);

                    case "Програмист":
                        return await HandleProgrammerCommand(message);

                    case "Нейросеть":
                        return await HandleNeuralNetworkCommand(message);

                    default:
                        return await SendUnknownCommandMessage(message);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Ошибка обработки команды: {ex.Message}");

                string errorText = $"Произошла ошибка при обработке команды: {ex.Message}!";

                return await _botClient.SendMessage(chatId: message.Chat.Id, text: errorText);
            }
        }

        return message;
    }


    private async Task<Message> SendStartKeyboard(Message message)
    {
        List<string> buttons = ["Переводчик", "Програмист", "Нейросеть"];

        ReplyKeyboardMarkup keyboard = _keyboardHandler.CreateBasicKeyboard(buttons, buttonsPerRow: 3);

        return await _botClient.SendMessage(
            chatId: message.Chat.Id,
            text: "Добро пожаловать! Я AI бот!\nВыберите команду:",
            replyMarkup: keyboard
        );
    }


    private async Task<Message> SendHelpMessage(Message message)
    {
        long chatId = message.Chat.Id;

        const string helpText = """
            Доступные команды:
            /start - Начать работу с ботом
            /help - Показать справку
            """;

        return await _botClient.SendMessage(chatId, helpText);
    }


    private async Task<Message> SendUnknownCommandMessage(Message message)
    {
        long chatId = message.Chat.Id;
        string text = "Используйте /help.";
        return await _botClient.SendMessage(chatId, text);
    }


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
            return await _botClient.SendMessage(chatId, $"```\n{result}\n```", ParseMode.MarkdownV2);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Ошибка {ex.Message}");
            return await _botClient.SendMessage(chatId, $"Ошибка {ex.Message}");
        }
    }


}