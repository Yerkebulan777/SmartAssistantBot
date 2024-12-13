using SmartAssistantBot.Interfaces;
using Telegram.Bot;

namespace SmartAssistantBot.Services;

// Compose Receiver and UpdateHandler implementation
public sealed class ReceiverService(ITelegramBotClient botClient, UpdateHandler updateHandler, ILogger<ReceiverServiceBase<UpdateHandler>> logger)
    : ReceiverServiceBase<UpdateHandler>(botClient, updateHandler, logger);


