using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;   
using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Configuration;

namespace OPSKWA.Services.Utilities
{
    internal class OllamaClient
    {
        private readonly string _baseUrl;
        private readonly HttpClient _httpClient;
        private readonly JsonSerializerOptions _jsonOptions;
        private string _model;

        public OllamaClient(string baseUrl = "http://localhost:11434")
        {
            _baseUrl = baseUrl.EndsWith("/") ? baseUrl.Substring(0, baseUrl.Length - 1) : baseUrl;
            _httpClient = new HttpClient
            {
                Timeout = TimeSpan.FromMinutes(5)
            };
            _jsonOptions = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
            };
            SetConfigurationContext();
        }

        public async Task<string> Generate(string prompt, bool stream = false)
        {
            var requestBody = new Dictionary<string, object>
            {
                { "model", _model },
                { "prompt", prompt },
                { "stream", stream }
            };

            string jsonBody = JsonSerializer.Serialize(requestBody, _jsonOptions);
            var content = new StringContent(jsonBody, Encoding.UTF8, "application/json");

            HttpResponseMessage response = await _httpClient.PostAsync($"{_baseUrl}/api/generate", content);

            if (!response.IsSuccessStatusCode)
            {
                throw new Exception($"Request failed with status: {response.StatusCode}");
            }

            string responseBody = await response.Content.ReadAsStringAsync();
            using JsonDocument jsonDoc = JsonDocument.Parse(responseBody);
            if (jsonDoc.RootElement.TryGetProperty("response", out var responseElement) && responseElement.ValueKind == JsonValueKind.String)
            {
                return responseElement.GetString()!;
            }
            throw new Exception("Response property not found or is null.");
        }

        public async Task<string> Chat(ChatMessage[] messages)
        {
            var requestBody = new Dictionary<string, object>
            {
                { "model", _model },
                { "messages", messages },
                { "stream", false }
            };

            string jsonBody = JsonSerializer.Serialize(requestBody, _jsonOptions);
            var content = new StringContent(jsonBody, Encoding.UTF8, "application/json");

            HttpResponseMessage response = await _httpClient.PostAsync($"{_baseUrl}/api/chat", content);

            if (!response.IsSuccessStatusCode)
            {
                throw new Exception($"Request failed with status: {response.StatusCode}");
            }

            string responseBody = await response.Content.ReadAsStringAsync();
            using JsonDocument jsonDoc = JsonDocument.Parse(responseBody);
            var contentElement = jsonDoc.RootElement.GetProperty("message").GetProperty("content");
            if (contentElement.ValueKind == JsonValueKind.String)
            {
                return contentElement.GetString()!;
            }
            throw new Exception("Content property not found or is null.");
        }

        private void SetConfigurationContext()
        {
            _model = ConfigurationManager.AppSettings["LLMService.ModelName"] ?? "qwen2.5:latest";
        }

        public async Task<JsonDocument> ListModels()
        {
            HttpResponseMessage response = await _httpClient.GetAsync($"{_baseUrl}/api/tags");
            string responseBody = await response.Content.ReadAsStringAsync();
            return JsonDocument.Parse(responseBody);
        }

        public async Task UnloadModel()
        {
            var requestBody = new Dictionary<string, object>
            {
                { "model", _model },
                { "keep_alive", 0 }
            };

            string jsonBody = JsonSerializer.Serialize(requestBody, _jsonOptions);
            var content = new StringContent(jsonBody, Encoding.UTF8, "application/json");

            await _httpClient.PostAsync($"{_baseUrl}/api/generate", content);
        }

        public class ChatMessage
        {
            [JsonPropertyName("role")]
            public string Role { get; set; }

            [JsonPropertyName("content")]
            public string Content { get; set; }

            public ChatMessage(string role, string content)
            {
                Role = role;
                Content = content;
            }
        }
    }   
}









