namespace Shared.Models;

public class ChatThreads
{
    public Guid Id { get; set; }
    public int MessageThreadId { get; set; }
    public string? ThreadSummary { get; set; }
}