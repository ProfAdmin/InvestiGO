using Microsoft.EntityFrameworkCore;
using Shared.Models;

namespace OpenAIConnector.Data;

public class AppDbContext: DbContext
{
    public DbSet<MessageRecord> Messages { get; set; }
    public DbSet<Group?> Groups { get; set; }

    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }
}