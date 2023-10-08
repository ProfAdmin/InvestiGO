using Microsoft.EntityFrameworkCore;
using Shared.Models;
using Telegram.Bot;
using TelegramBot.Data;
using TelegramBot.Services;

namespace TelegramBot;

public class Bot
{
    private readonly TelegramBotClient _botClient;
    private readonly OpenAIService _openAIService;
    private readonly AppDbContext _dbContext;
    private int _lastUpdateId;
    private string? _summaryHeadline;
    
    // Even though this is not used, we need this variable alive for our poll mechanism
    // ReSharper disable once NotAccessedField.Local
    private Timer _timer = null!;

    public Bot(string telegramApiKey, string openAiApiKey, AppDbContext dbContext)
    {
        _botClient = new TelegramBotClient(telegramApiKey);
        _openAIService = new OpenAIService(openAiApiKey);
        _dbContext = dbContext;
    }

    public void Start()
    {
        // Get the latest stored message so we don't record messages twice
        _lastUpdateId = _dbContext.Messages
            .DefaultIfEmpty()
            .Max(x => x!.LastUpdateId);
        
        // check every 10 seconds
        _timer = new Timer(CheckForUpdates!, null, TimeSpan.Zero, TimeSpan.FromSeconds(10));
    }

    private async void CheckForUpdates(object state)
    {
        Console.WriteLine($"--> Telegram Bot Console App --> {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss} UTC: Polling for messages...");

        var updates = await _botClient.GetUpdatesAsync(_lastUpdateId + 1);

        foreach (var update in updates)
        {
            _lastUpdateId = update.Id;

            if (update.Message == null) continue;

            var command = update.Message.Text; 
                
            if (command?.StartsWith("/register") ?? false)
            {
                await RegisterGroupAsync(update.Message.Chat.Id);
            }
            else if (command?.StartsWith("/unregister") ?? false)
            {
                await UnregisterGroupAsync(update.Message.Chat.Id);
            }
            else if (command?.StartsWith("/summary") ?? false)
            {
                await SummaryGroupAsync(command, update.Message.Chat.Id);
            }
            else
            {
                // Read the groupId from the chat message
                var groupId = update.Message.Chat.Id;

                // Get the group from DB making sure it is active
                var group = await _dbContext.Groups
                    .FirstOrDefaultAsync(x => x.ChatId == groupId && x.IsActive);

                string? messageText = update.Message.Text ?? update.Message.Caption;

                // Ignore messages that don't have text, or when the group is not registered
                if (group == null || messageText == null) continue;

                // Store the message in DB if the group is registered
                var messageRecord = new MessageRecord
                {
                    Id = Guid.NewGuid(),
                    ChatId = groupId,
                    MessageId = update.Message.MessageId,
                    Text = messageText,
                    Date = update.Message.Date,
                    LastUpdateId = _lastUpdateId,
                    SenderId = update.Message.From?.Id ?? 0,
                    SenderUsername = update.Message.From?.Username ?? string.Empty,
                    ThreadId = update.Message.MessageThreadId ?? 0,
                };

                _dbContext.Messages.Add(messageRecord);
                await _dbContext.SaveChangesAsync();
            }
        }
    }

    private async Task SummaryGroupAsync(string command, long chatId)
    {
        await _botClient.SendTextMessageAsync(chatId, "Turning verbosity into brevity... Please wait some seconds!");
        
        int numberOfMessagesToSummarize;

        if (command.StartsWith("/summary "))
        {
            // Remove "/summary " from the command
            var numberString = command.Replace("/summary ", "");

            // Try to parse the remaining string as an integer
            // If the parsing is successful, set the number of messages to summarize to the parsed integer
            // If the parsing fails, get the number of messages from today
            if (int.TryParse(numberString, out var number))
            {
                _summaryHeadline = $"Summary for the last {number} messages:";
                numberOfMessagesToSummarize = number;
            }
            else
            {
                _summaryHeadline = "Summary for all the messages of today:";
                numberOfMessagesToSummarize = await GetMessageCountFromTodayAsync(chatId);
            }
        }
        else
        {
            // If the command is "/summary", get the number of messages from today
            _summaryHeadline = "Summary for all the messages of today:";
            numberOfMessagesToSummarize = await GetMessageCountFromTodayAsync(chatId);
        }

        var firstMessageId = await _dbContext.Messages
            .Where(m => m.ChatId == chatId)
            .OrderByDescending(m => m.MessageId)
            .Skip(numberOfMessagesToSummarize - 1)
            .Select(m => m.MessageId)
            .FirstOrDefaultAsync();

        var summary = await GetSummaryAsync(firstMessageId, numberOfMessagesToSummarize, chatId);

        var formattedSummary = $"{_summaryHeadline}\n\n {summary}";
        
        await _botClient.SendTextMessageAsync(chatId, formattedSummary);
    }

    private async Task<string> GetSummaryAsync(int firstMessageId, int count, long chatId)
    {
        var existingSummary = await _dbContext.Summaries
            .Where(x => x.FirstMessageId == firstMessageId
                        && x.MessagesCount == count)
            .FirstOrDefaultAsync();

        // If we already found a summary in our Database, no need to call GPT at all
        if (existingSummary != null) 
            return existingSummary.SummaryText ?? string.Empty;

        var messages = await _dbContext.Messages
            .Where(m => m.ChatId == chatId && m.MessageId > firstMessageId)
            .OrderBy(m => m.MessageId)
            .Take(count)
            .ToListAsync();

        var summary = await _openAIService.GetSummary(messages);

        await StoreSummaryInDb(firstMessageId, count, chatId, summary);

        return summary;
    }

    private async Task StoreSummaryInDb(int firstMessageId, int count, long chatId, string summary)
    {
        var newSummary = new Summary
        {
            Id = Guid.NewGuid(),
            ChatId = chatId,
            SummaryText = summary,
            FirstMessageId = firstMessageId,
            MessagesCount = count
        };

        _dbContext.Summaries.Add(newSummary);
        await _dbContext.SaveChangesAsync();
    }

    private async Task<int> GetMessageCountFromTodayAsync(long chatId)
    {
        // Get today's date at 00:00 UTC
        DateTime todayUtc = DateTime.UtcNow.Date;

        // Get the number of messages from today
        return await _dbContext.Messages
            .Where(m => m.ChatId == chatId && m.Date >= todayUtc)
            .CountAsync();
    }

    private async Task RegisterGroupAsync(long groupId)
    {
        // Check if the group is already registered
        var existingGroup = await _dbContext.Groups.FirstOrDefaultAsync(x => x.ChatId == groupId);

        if (existingGroup == null)
        {
            var group = new Group
            {
                Id = Guid.NewGuid(),
                ChatId = groupId,
                IsActive = true,
            };
            _dbContext.Groups.Add(group);
        }
        else
        {
            existingGroup.IsActive = true;
        }

        await _dbContext.SaveChangesAsync();
        await _botClient.SendTextMessageAsync(groupId, "Group registered successfully!");
    }

    private async Task UnregisterGroupAsync(long groupId)
    {
        var existingGroup = await _dbContext.Groups.FirstOrDefaultAsync(x => x.ChatId == groupId);

        if (existingGroup != null)
        {
            existingGroup.IsActive = false;
            await _dbContext.SaveChangesAsync();
            await _botClient.SendTextMessageAsync(groupId, "Group unregistered successfully!");
        }
        else
        {
            await _botClient.SendTextMessageAsync(groupId, "Group is not registered.");
        }
    }
}