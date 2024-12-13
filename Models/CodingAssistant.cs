using LLama.Common;
using LLama.Sampling;
using Microsoft.Extensions.Options;
using SmartAssistantBot.Core;


namespace SmartAssistantBot.Models;
public sealed class CodingAssistant : BaseAssistant
{
    protected override string SystemPrompt =>
    @"Вы опытный программист, специализирующийся на:
            - Написании чистого, эффективного и поддерживаемого кода
            - Следовании лучшим практикам и стандартам кодирования
            - Предоставлении понятных объяснений и документации
            - Предложении оптимальных решений программных задач
            - Выявлении потенциальных проблем и проблем безопасности
            - Использовании современных паттернов программирования";


    protected override ModelParams GetModelParameters(BotConfiguration config)
    {
        Logger?.LogDebug("Инициализация модели. Путь: {ModelPath}", config.CodingModelPath);

        if (string.IsNullOrEmpty(config.CodingModelPath))
        {
            throw new ArgumentNullException("modelPath");
        }

        return new ModelParams(config.CodingModelPath)
        {
            ContextSize = 4096,
            GpuLayerCount = 10,
            BatchSize = 512,
        };
    }


    protected override InferenceParams GetDefaultInferenceParams()
    {
        InferenceParams baseParams = base.GetDefaultInferenceParams();

        baseParams.SamplingPipeline = new DefaultSamplingPipeline()
        {
            Temperature = 0.2f
        };

        baseParams.MaxTokens = -1;

        return baseParams;
    }


    public CodingAssistant(ILogger<CodingAssistant> logger, IOptions<BotConfiguration> config) : base(logger, config)
    {
        /// Реализация конструктора в абстрактном классе
    }


}