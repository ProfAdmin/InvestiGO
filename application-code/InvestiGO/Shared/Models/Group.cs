namespace Shared.Models;

public class Group
{
    public Guid Id { get; set; }
    public long ChatId { get; set; }
    public bool IsActive { get; set; }
}