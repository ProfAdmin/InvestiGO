using Microsoft.EntityFrameworkCore;
using Telegram.Bot;
using Telegram.Bot.Types.Enums;
using TelegramBot.Data;
using TelegramBot.Models;

namespace TelegramBot;

public class Bot
{
    private readonly TelegramBotClient _botClient;
    private readonly AppDbContext _dbContext;
    private int _lastUpdateId;
    
    // Even though this is not used, we need this variable alive for our poll mechanism
    // ReSharper disable once NotAccessedField.Local
    private Timer _timer = null!;

    public Bot(string apiKey, AppDbContext dbContext)
    {
        _botClient = new TelegramBotClient(apiKey);
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
                
            if (update.Message.Text?.StartsWith("/register") ?? false)
            {
                await RegisterGroupAsync(update.Message.Chat.Id);
            }
            else if (update.Message.Text?.StartsWith("/unregister") ?? false)
            {
                await UnregisterGroupAsync(update.Message.Chat.Id);
            }
            else
            {
                // Read the groupId from the chat message
                var groupId = update.Message.Chat.Id;

                // Get the group from DB making sure it is active
                var group = await _dbContext.Groups
                    .FirstOrDefaultAsync(x => x.ChatId == groupId && x.IsActive);
                    
                // Ignore messages that are not text, or when the group is not registered
                if (group == null || update.Message.Type != MessageType.Text) continue;

                // Store the message in DB if the group is registered
                var messageRecord = new MessageRecord
                {
                    Id = Guid.NewGuid(),
                    ChatId = groupId,
                    MessageId = update.Message.MessageId,
                    Text = update.Message.Text,
                    Date = update.Message.Date,
                    LastUpdateId = _lastUpdateId,
                    SenderId = update.Message.SenderChat?.Id ?? 0,
                    SenderUsername = update.Message.SenderChat?.Username ?? string.Empty,
                    SenderType = update.Message.SenderChat?.Type ?? null
                };

                _dbContext.Messages.Add(messageRecord);
                await _dbContext.SaveChangesAsync();
            }
        }
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