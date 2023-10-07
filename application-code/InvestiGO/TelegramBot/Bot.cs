using Telegram.Bot;
using Telegram.Bot.Types.Enums;
using TelegramBot.Data;
using TelegramBot.Models;

namespace TelegramBot;

public class Bot
{
    private readonly TelegramBotClient _botClient;
    private readonly AppDbContext _dbContext;
    private readonly long _groupId;
    private int _lastUpdateId;

    public Bot(string apiKey, AppDbContext dbContext, long groupId)
    {
        _botClient = new TelegramBotClient(apiKey);
        _dbContext = dbContext;
        _groupId = groupId;
    }

    public void Start()
    {
        // check every 10 seconds
        var timer = new Timer(CheckForUpdates!, null, TimeSpan.Zero, TimeSpan.FromSeconds(10));
    }

    private async void CheckForUpdates(object state)
    {
        var updates = await _botClient.GetUpdatesAsync(_lastUpdateId + 1);
        foreach (var update in updates)
        {
            _lastUpdateId = update.Id;
            if (update.Message != null && update.Message.Chat.Id == _groupId && update.Message.Type == MessageType.Text)
            {
                var messageRecord = new MessageRecord
                {
                    Id = Guid.NewGuid(),
                    ChatId = update.Message.Chat.Id,
                    MessageId = update.Message.MessageId,
                    Text = update.Message.Text,
                    Date = update.Message.Date,
                    SenderId = update.Message.SenderChat?.Id ?? 0,
                    SenderUsername = update.Message.SenderChat?.Username ?? string.Empty,
                    SenderType = update.Message.SenderChat?.Type ?? null
                };
                
                _dbContext.Messages.Add(messageRecord);
                await _dbContext.SaveChangesAsync();
            }
        }
    }
}