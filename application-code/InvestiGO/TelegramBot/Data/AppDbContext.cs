using TelegramBot.Models;

namespace TelegramBot.Data;

using Microsoft.EntityFrameworkCore;

public class AppDbContext : DbContext
{
    public DbSet<MessageRecord> Messages { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseInMemoryDatabase("TelegramMessages");
    }
}
