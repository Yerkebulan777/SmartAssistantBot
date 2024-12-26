using Telegram.Bot.Types;


namespace SmartAssistantBot.Interfaces
{
    /// <summary>
    /// Интерфейс для обработки сообщений бота
    /// </summary>
    public interface IMessageHandler
    {
        /// <summary>
        /// Обработка команд
        /// </summary>
        Task<Message> OnMessage(Message message, CancellationToken cancellationToken);
    }
}
