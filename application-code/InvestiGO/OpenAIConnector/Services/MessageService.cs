using Microsoft.EntityFrameworkCore;
using OpenAIConnector.Data;
using Shared.Models;


namespace OpenAIConnector.Services;

public class MessageService
{
    private readonly AppDbContext _dbContext;

    public MessageService(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<List<MessageRecord>> GetMessagesAsync(long groupId)
    {
        var messages = await _dbContext.Messages
            .Where(m => m.ChatId == groupId)
            .ToListAsync();
        return messages;
    }
}