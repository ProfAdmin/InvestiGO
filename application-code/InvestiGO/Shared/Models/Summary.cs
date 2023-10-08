namespace Shared.Models;

public class Summary
{
    public Guid Id { get; set; }
    public long ChatId { get; set; }
    public string? SummaryText { get; set; }
    public long FirstMessageId { get; set; }
    public int MessagesCount { get; set; }
}