using Telegram.Bot.Types;

namespace SmartAssistantBot.Interfaces;

public interface IKeyboardHandler
{
    /// <summary>
    /// Отправляет клавиатуру с одним рядом кнопок.
    /// </summary>
    /// <param name="msg">Сообщение, на основе которого будет отправлена клавиатура</param>
    /// <param name="text">Текст сообщения</param>
    /// <param name="buttonTexts">Список текстов для кнопок одного ряда</param>
    /// <returns>Отправленное сообщение с клавиатурой</returns>
    Task<Message> SendSingleRowReplyKeyboard(Message msg, string text, List<string> buttonTexts);

    /// <summary>
    /// Отправляет инлайн клавиатуру.
    /// </summary>
    /// <param name="msg">Сообщение, на основе которого будет отправлена клавиатура</param>
    /// /// <param name="baseText">Текст сообщения</param>
    /// <param name="textAndCallbackData">Словарь с текстами кнопок и их callback-данными</param>
    /// <returns>Отправленное сообщение с клавиатурой</returns>
    Task<Message> SendInlineColumnKeyboard(Message msg, string baseText, Dictionary<string, string> textAndCallbackData);

    /// <summary>
    /// Обрабатывает callback запросы от инлайн-кнопок.
    /// </summary>
    /// <param name="callbackQuery">Объект callback запроса</param>
    /// <returns>Task</returns>
    Task OnCallbackQuery(CallbackQuery callbackQuery);

    /// <summary>
    /// Обрабатывает инлайн-запросы.
    /// </summary>
    /// <param name="inlineQuery">Объект инлайн-запроса</param>
    Task OnInlineQuery(InlineQuery inlineQuery);
}