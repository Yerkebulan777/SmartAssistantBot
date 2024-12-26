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
    Task<Message> SendSingleRowKeyboard(Message msg, string text, List<string> buttonTexts);

    /// <summary>
    /// Отправляет инлайн клавиатуру.
    /// </summary>
    /// <param name="msg">Сообщение, на основе которого будет отправлена клавиатура</param>
    /// <param name="buttonTextAndCallbackData">Словарь с текстами кнопок и их callback-данными</param>
    /// <param name="buttonsPerRow">Количество кнопок в одном ряду (по умолчанию 2)</param>
    /// <returns>Отправленное сообщение с клавиатурой</returns>
    Task<Message> SendInlineKeyboard(Message msg, Dictionary<string, string> buttonTextAndCallbackData, int buttonsPerRow = 2);

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