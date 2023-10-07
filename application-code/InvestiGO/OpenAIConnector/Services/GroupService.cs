using Microsoft.EntityFrameworkCore;
using OpenAIConnector.Data;
using Shared.Models;

namespace OpenAIConnector.Services;

public class GroupService
{
    private readonly AppDbContext _dbContext;

    public GroupService(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Group?> GetFirsGroupAsync()
    {
        return await _dbContext.Groups.FirstOrDefaultAsync();
    }
}