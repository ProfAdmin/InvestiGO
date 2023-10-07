using OpenAI;
using OpenAI.Chat;
using Shared.Models;

namespace OpenAIConnector.Services;

public class OpenAIService
{
    private readonly string _apiKey;

    public OpenAIService(string apiKey)
    {
        _apiKey = apiKey;
    }

    public async Task<string> GetSummary(List<MessageRecord> dbMessages)
    {
        var api = new OpenAIClient(_apiKey);
        
        string concatenatedMessages = string.Join(" | ", dbMessages.Select(m => m.Text));

        var messages = new List<Message>
        {
            new(Role.System, "You are a helpful assistant."),
            new(Role.User, "I will send you a list of messages from a telegram group chat. Your task is to read them all and give me a summary with the most important points. Each message will be separated from each other by the pipe character ( | )"),
            new(Role.User, concatenatedMessages)
        };
        
        var chatRequest = new ChatRequest(messages);
        var result = await api.ChatEndpoint.GetCompletionAsync(chatRequest);
        
        return result;
    }
}