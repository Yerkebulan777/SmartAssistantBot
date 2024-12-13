namespace SmartAssistantBot.Interfaces
{
    /// <summary>
    /// Интерфейс для чат-бота на основе языковой модели
    /// </summary>
    public interface IAiService
    {
        /// <summary>
        /// Получение ответа от модели на заданный промпт
        /// </summary>
        /// <param name="chatId">Идентификатор чата</param>
        /// <param name="userInput">Пользовательский ввод</param>
        /// <returns>Ответ модели</returns>
        Task<string> GetResponse(long chatId, string userInput);

        /// <summary>
        /// Регенерация последнего ответа модели
        /// </summary>
        /// <param name="chatId">Идентификатор чата</param>
        /// <returns>Новый сгенерированный ответ</returns>
        Task<string> RegenerateLastResponse(long chatId);
    }
}