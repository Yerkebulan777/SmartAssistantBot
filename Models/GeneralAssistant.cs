using LLama.Common;
using LLama.Sampling;
using Microsoft.Extensions.Options;
using SmartAssistantBot.Core;


namespace SmartAssistantBot.Models;
public sealed class GeneralAssistant : BaseAssistant
{
    protected override string SystemPrompt =>
    @"Вы полезный ИИ-ассистент, который:
        - Предоставляет информативные и увлекательные ответы
        - Поддерживает естественный поток беседы
        - Адаптирует стиль общения под потребности пользователя
        - Предлагает полезные советы и решения
        - Компетентно обрабатывает широкий спектр тем
        - Фокусируется на целях и вопросах пользователя";


    protected override ModelParams GetModelParameters(BotConfiguration config)
    {
        if (string.IsNullOrEmpty(config.GeneralModelPath))
        {
            throw new ArgumentNullException("modelPath");
        }

        return new ModelParams(config.GeneralModelPath)
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
            Temperature = 0.7f
        };

        baseParams.MaxTokens = 1000;

        return baseParams;
    }

    public GeneralAssistant(ILogger<CodingAssistant> logger, IOptions<BotConfiguration> config) : base(logger, config)
    {
        /// Реализация конструктора в абстрактном классе
    }
}

