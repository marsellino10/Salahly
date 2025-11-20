using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SalahlyProject.Contracts.Chat;
using SalahlyProject.Options;

namespace SalahlyProject.Services.Chat
{
    public class FireworksChatService : IChatService
    {
        private const string EndpointPath = "chat/completions";
        private const string FallbackMessage = "Thanks for asking! I'm not sure about that yet, but our support team would love to help if you reach out through the in-app support form.";

        private static readonly JsonSerializerOptions SerializerOptions = new()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
            WriteIndented = false,
        };

        private static readonly string SystemPrompt =
            "You are Salahly's friendly in-app customer assistant. Your goals are to: " +
            "(1) greet customers warmly and conversationally, " +
            "(2) answer questions using ONLY the provided Salahly application context (models, DTOs, services, controllers, validation attributes, business logic, or included knowledge base files), " +
            "(3) guide customers through account access, service booking, and technician support, and " +
            "(4) encourage next steps in the Salahly app. " +
            "If a customer greets you (e.g., 'hello', 'hi', 'good morning'), respond enthusiastically and offer help right away. " +
            "If the requested information is not present in the provided context, reply with the exact sentence: \"Thanks for asking! I'm not sure about that yet, but our support team would love to help if you reach out through the in-app support form.\" " +
            "Never invent features or knowledge beyond the shared context, and keep answers concise, positive, and customer-friendly.";

        private readonly HttpClient _httpClient;
        private readonly FireworksOptions _options;
        private readonly IChatContextBuilder _contextBuilder;
        private readonly ILogger<FireworksChatService> _logger;

        public FireworksChatService(
            HttpClient httpClient,
            IOptions<FireworksOptions> options,
            IChatContextBuilder contextBuilder,
            ILogger<FireworksChatService> logger)
        {
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
            _options = options?.Value ?? throw new ArgumentNullException(nameof(options));
            _contextBuilder = contextBuilder ?? throw new ArgumentNullException(nameof(contextBuilder));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<(string Answer, bool IsFallback)> AskAsync(string question, string? context, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(question))
            {
                return (FallbackMessage, true);
            }

            if (string.IsNullOrWhiteSpace(_options.ApiKey))
            {
                _logger.LogWarning("Fireworks API key is not configured.");
                return (FallbackMessage, true);
            }

            var aggregatedContext = await _contextBuilder.BuildContextAsync(question, context, cancellationToken);

            var messages = new List<FireworksMessage>
            {
                new("system", SystemPrompt)
            };

            if (!string.IsNullOrWhiteSpace(aggregatedContext))
            {
                messages.Add(new FireworksMessage("system", $"Application Context:\n{aggregatedContext}"));
            }

            messages.Add(new FireworksMessage("user", question));

            var request = new FireworksChatCompletionRequest
            {
                Model = _options.Model,
                Messages = messages,
                MaxTokens = 600,
                Temperature = 0.0,
                TopP = 0.9,
            };

            try
            {
                _logger.LogInformation("Sending request to Fireworks API with model: {Model}", _options.Model);

                using var httpRequest = new HttpRequestMessage(HttpMethod.Post, EndpointPath)
                {
                    Content = JsonContent.Create(request, mediaType: new MediaTypeHeaderValue("application/json"), options: SerializerOptions)
                };

                httpRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _options.ApiKey);
                httpRequest.Headers.Accept.Clear();
                httpRequest.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                // Log the request content for debugging
                var requestContent = await httpRequest.Content.ReadAsStringAsync(cancellationToken);
                _logger.LogDebug("Request body: {RequestBody}", requestContent);

                using var response = await _httpClient.SendAsync(httpRequest, cancellationToken);

                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync(cancellationToken);
                    _logger.LogError("Fireworks completion failed with status {StatusCode}: {Content}", (int)response.StatusCode, errorContent);
                    return (FallbackMessage, true);
                }

                var completion = await response.Content.ReadFromJsonAsync<FireworksChatCompletionResponse>(SerializerOptions, cancellationToken);
                var answer = completion?.Choices?.FirstOrDefault()?.Message?.Content?.Trim();

                if (string.IsNullOrWhiteSpace(answer))
                {
                    _logger.LogWarning("Fireworks completion returned an empty answer.");
                    return (FallbackMessage, true);
                }

                var isFallback = string.Equals(answer, FallbackMessage, StringComparison.OrdinalIgnoreCase);
                return (answer, isFallback);
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while calling Fireworks completion endpoint.");
                return (FallbackMessage, true);
            }
        }

        private record FireworksMessage(string Role, string Content);

        private record FireworksChatCompletionRequest
        {
            [JsonPropertyName("model")]
            public string Model { get; init; } = string.Empty;

            [JsonPropertyName("messages")]
            public IList<FireworksMessage> Messages { get; init; } = new List<FireworksMessage>();

            [JsonPropertyName("max_tokens")]
            public int MaxTokens { get; init; } = 512;

            [JsonPropertyName("temperature")]
            public double Temperature { get; init; } = 0.2;

            [JsonPropertyName("top_p")]
            public double TopP { get; init; } = 0.9;
        }

        private record FireworksChatCompletionResponse
        {
            [JsonPropertyName("choices")]
            public IList<FireworksChoice> Choices { get; init; } = Array.Empty<FireworksChoice>();
        }

        private record FireworksChoice
        {
            [JsonPropertyName("message")]
            public FireworksMessage? Message { get; init; }
        }
    }
}
