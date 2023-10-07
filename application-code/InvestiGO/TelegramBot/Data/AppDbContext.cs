using Microsoft.EntityFrameworkCore;
using TelegramBot.Models;

namespace TelegramBot.Data
{
    public class AppDbContext : DbContext
    {
        public DbSet<MessageRecord> Messages { get; set; }

        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options)
        {
        }
    }
}