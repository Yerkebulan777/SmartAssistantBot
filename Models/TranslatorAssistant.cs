using LLama.Common;
using LLama.Sampling;
using Microsoft.Extensions.Options;
using SmartAssistantBot.Core;


namespace SmartAssistantBot.Models;
public class TranslatorAssistant : BaseAssistant
{
    protected override string SystemPrompt => 
    @"Вы опытный помощник по программированию, специализирующийся на:
        - Написании чистого, эффективного и поддерживаемого кода
        - Следовании лучшим практикам и стандартам кодирования
        - Предоставлении понятных объяснений и документации
        - Предложении оптимальных решений программных задач
        - Выявлении потенциальных проблем и проблем безопасности
        - Использовании современных паттернов программирования";


    protected override ModelParams GetModelParameters(BotConfiguration config)
    {
        if (string.IsNullOrEmpty(config.TranslatorModelPath))
        {
            throw new ArgumentNullException("modelPath");
        }

        return new ModelParams(config.TranslatorModelPath)
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
            Temperature = 0.3f
        };

        baseParams.MaxTokens = 1000;

        return baseParams;
    }


    public TranslatorAssistant(ILogger<CodingAssistant> logger, IOptions<BotConfiguration> config) : base(logger, config)
    {
        /// Реализация конструктора в абстрактном классе
    }



}