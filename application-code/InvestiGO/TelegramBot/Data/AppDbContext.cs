using Microsoft.EntityFrameworkCore;
using Shared.Models;

namespace TelegramBot.Data
{
    public class AppDbContext : DbContext
    {
        public DbSet<MessageRecord> Messages { get; set; }
        public DbSet<Group> Groups { get; set; }
        public DbSet<Summary> Summaries { get; set; }
        public DbSet<ChatThreads> ChatThreads { get; set; }

        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options)
        {
        }
    }
}