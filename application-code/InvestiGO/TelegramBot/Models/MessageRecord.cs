using Telegram.Bot.Types.Enums;

namespace TelegramBot.Models;

public class MessageRecord
{
    public int Id { get; set; }
    public long ChatId { get; set; }
    public int MessageId { get; set; }
    public string? Text { get; set; }
    public DateTime Date { get; set; }
    public long SenderId { get; set; }
    public string? SenderUsername { get; set; }
    public ChatType? SenderType { get; set; }
}