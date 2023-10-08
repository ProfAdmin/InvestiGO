using OpenAI;
using OpenAI.Chat;
using Shared.Models;

namespace TelegramBot.Services;

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
    
        string concatenatedMessages = string.Join
        (
            " | ", 
            dbMessages.Select(m => $"{m.SenderUsername}: {m.Text}")
        );
        
        // check if string length is more than 13.000 characters. This roughly corresponds to chatgpt 4k token limitation
        if (concatenatedMessages.Length > 13_000)
        {
            // truncate the string to its first 13.000 characters
            concatenatedMessages = concatenatedMessages.Substring(0, 13_000);
        }

        var messages = new List<Message>
        {
            new(Role.System, "You are a helpful assistant and an expert in summarizing. Take into account that the point of your work is to provide SHORTER texts than the input."),
            new(Role.User, "I will send you a list of messages from a telegram group chat. Your task is to read them all and give me a summary with the most important points. Each message will be separated from each other by the pipe character ( | ). Messages will include the username as well. If you detect this is a conversation between 2 or more people, do not give a summary of each message, but rather of the main point of the conversation. Separate your answer in bullet points"),
            new(Role.User, concatenatedMessages)
        };

        var chatRequest = new ChatRequest(messages);
        var result = await api.ChatEndpoint.GetCompletionAsync(chatRequest);
    
        // turn result to a string
        string resultString = result.ToString() ?? string.Empty;

        // split the string into words
        string[] words = resultString.Split(' ');

        // check if word count is more than 100
        if (words.Length > 100)
        {
            // make a second call to the API
            var conciseRequest = new ChatRequest(new List<Message>
            {
                new(Role.User, "Please provide a more concise summary with less than 100 words of the following text. Use bullet points for the answer."),
                new(Role.User, resultString)
            });

            result = await api.ChatEndpoint.GetCompletionAsync(conciseRequest);
        }
    
        return result;
    }
}