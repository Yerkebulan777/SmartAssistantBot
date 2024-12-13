using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;


namespace SmartAssistantBot.Interfaces;
public interface IKeyboardHandler
{
    /// <summary>
    /// Создает базовую клавиатуру с обычными кнопками.
    /// </summary>
    /// <param name="buttonTexts">Список текстов для кнопок</param>
    /// <param name="buttonsPerRow">Количество кнопок в одном ряду</param>
    /// <returns>Объект клавиатуры ReplyKeyboardMarkup</returns>
    ReplyKeyboardMarkup CreateBasicKeyboard(List<string> buttonTexts, int buttonsPerRow = 2);

    /// <summary>
    /// Создает инлайн-клавиатуру с кнопками обратного вызова.
    /// </summary>
    /// <param name="buttonTextAndCallbackData">Словарь с текстами кнопок и их callback-данными</param>
    /// <param name="buttonsPerRow">Количество кнопок в одном ряду</param>
    /// <returns>Объект клавиатуры InlineKeyboardMarkup</returns>
    InlineKeyboardMarkup CreateInlineKeyboard(Dictionary<string, string> buttonTextAndCallbackData, int buttonsPerRow = 2);

    /// <summary>
    /// Создает специальную клавиатуру с кнопками запроса контакта и/или локации.
    /// </summary>
    /// <param name="requestContact">Флаг для добавления кнопки запроса контакта</param>
    /// <param name="requestLocation">Флаг для добавления кнопки запроса локации</param>
    /// <returns>Объект клавиатуры ReplyKeyboardMarkup</returns>
    ReplyKeyboardMarkup CreateSpecialKeyboard(bool requestContact = false, bool requestLocation = false);


    Task HandleCallbackQuery(CallbackQuery callbackQuery);


    Task HandleInlineQuery(InlineQuery inlineQuery);


    /// <summary>
    /// Удаляет текущую клавиатуру
    /// </summary>
    /// <param name="chatId">ID чата</param>
    /// <param name="message">Сообщение для отправки</param>
    /// <returns>Task</returns>
    Task RemoveKeyboard(long chatId);



}