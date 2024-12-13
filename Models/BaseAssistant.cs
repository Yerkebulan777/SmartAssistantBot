using LLama;
using LLama.Common;
using LLama.Sampling;
using Microsoft.Extensions.Options;
using SmartAssistantBot.Core;
using SmartAssistantBot.Interfaces;
using System.Text;


namespace SmartAssistantBot.Models;
public abstract class BaseAssistant : IAiService, IDisposable
{
    private readonly string _statePath;
    private readonly ModelParams _parameters;
    private readonly ILogger<BaseAssistant> _logger;

    private ChatSession? currentSession;

    protected abstract string SystemPrompt { get; }

    protected abstract ModelParams GetModelParameters(BotConfiguration config);

    protected virtual InferenceParams GetDefaultInferenceParams()
    {
        return new InferenceParams
        {
            SamplingPipeline = new DefaultSamplingPipeline(),
            AntiPrompts = new[] { "User:" },
            MaxTokens = 512
        };
    }


    protected BaseAssistant(ILogger<BaseAssistant> logger, IOptions<BotConfiguration> config)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        if (config?.Value is null)
        {
            throw new ArgumentNullException(nameof(config));
        }

        Type chatBotType = GetType();
        string directory = AppDomain.CurrentDomain.BaseDirectory;
        string sessionDir = Path.Combine(directory, chatBotType.Name);

        if (!Directory.Exists(sessionDir))
        {
            _ = Directory.CreateDirectory(sessionDir);
        }

        _parameters = GetModelParameters(config.Value);

        _statePath = Path.Combine(sessionDir, $"session_{DateTime.Now:yyyyMMdd_HHmmss}");

        InitialSession();
    }


    public void InitialSession()
    {
        try
        {
            using LLamaWeights model = LLamaWeights.LoadFromFile(_parameters);
            using LLamaContext context = model.CreateContext(_parameters);
            InteractiveExecutor executor = new(context);

            ChatHistory chatHistory = new();
            chatHistory.AddMessage(AuthorRole.System, SystemPrompt);

            currentSession = new(executor, chatHistory);
            currentSession.SaveSession(_statePath);

            _logger.LogInformation($"Сессия успешно инициализирована. Путь к сессии: {_statePath}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка инициализации сессии");
            throw;
        }
    }


    public async Task<string> GetResponse(long chatId, string userInput)
    {
        _logger.LogDebug("Обработка сообщения в чате {ChatId}", chatId);

        try
        {
            using LLamaWeights model = await LLamaWeights.LoadFromFileAsync(_parameters);
            using LLamaContext context = model.CreateContext(_parameters);
            InteractiveExecutor executor = new(context);

            currentSession = new(executor);
            currentSession.LoadSession(_statePath);

            ChatHistory.Message message = new(AuthorRole.User, userInput);

            StringBuilder response = new();

            _ = response.AppendLine($"### {AuthorRole.User}: {userInput}");

            await foreach (string text in currentSession.ChatAsync(message, GetDefaultInferenceParams()))
            {
                _ = response.AppendLine($"### {AuthorRole.Assistant}: {text}");
            }

            currentSession.SaveSession(_statePath);

            return response.ToString();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка генерации ответа для чата {ChatId}", chatId);
            throw;
        }
    }


    public async Task<string> RegenerateLastResponse(long chatId)
    {
        _logger.LogDebug("Повторная генерация ответа в чате {ChatId}", chatId);

        try
        {
            using LLamaWeights model = await LLamaWeights.LoadFromFileAsync(_parameters);
            using LLamaContext context = model.CreateContext(_parameters);
            InteractiveExecutor executor = new(context);

            currentSession = new(executor);
            currentSession.LoadSession(_statePath);

            StringBuilder response = new();

            await foreach (string text in currentSession.RegenerateAssistantMessageAsync(GetDefaultInferenceParams()))
            {
                _ = response.AppendLine($"### {AuthorRole.Assistant}: {text}");
            }

            currentSession.SaveSession(_statePath);

            return response.ToString();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка повторной генерации ответа в чате {ChatId}", chatId);
            throw;
        }
    }


    public void Dispose()
    {
        try
        {
            /// ?Dispose();
            GC.SuppressFinalize(this);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка освобождения ресурсов");
        }
    }



}