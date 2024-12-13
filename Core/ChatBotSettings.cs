namespace SmartAssistantBot.Core
{
    public class ChatBotSettings
    {
        public int GpuLayers { get; set; } = 10; /// Количество слоев для GPU обработки
        public int ContextSize { get; set; } = 4096; /// Размер контекста для обработки
        public float Temperature { get; set; } = 0.75f; /// Температура генерации (креативность ответов)
        public string ModelPath { get; set; } = string.Empty; /// Путь к файлу модели
        public string InitialPrompt { get; set; } = string.Empty; /// Начальный промпт для инициализации модели
        public int MaxTokens { get; set; } = -1;  /// Максимальное количество токенов в ответе (-1 = без ограничений)
        public bool UseGPU { get; set; } = true; /// Использовать ли GPU для вычислений
        public int Seed { get; set; } = 40; /// Seed для генератора случайных чисел


        public static ChatBotSettings Default => new()
        {
            InitialPrompt = "Я помощник искусственного интеллекта.",
            ModelPath = "models/llama-2-7b-chat.gguf",
            GpuLayers = 10,
            ContextSize = 4096,
            Temperature = 0.7f,
            MaxTokens = -1,
            UseGPU = false,
            Seed = 40,
        };


    }

}

