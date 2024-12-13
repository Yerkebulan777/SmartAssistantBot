namespace SmartAssistantBot.Core;


public class BotConfiguration
{
    public string BotToken { get; init; } = default!;
    public string TranslatorModelPath { get; init; } = default!;
    public string GeneralModelPath { get; init; } = default!;
    public string CodingModelPath { get; init; } = default!;
}
