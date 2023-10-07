using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using TelegramBot;
using TelegramBot.Data;

var configuration = new ConfigurationBuilder()
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .Build();

var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();
optionsBuilder.UseSqlServer(configuration.GetConnectionString("DefaultConnection"));

using var dbContext = new AppDbContext(optionsBuilder.Options);

// Automatically apply any pending migrations
dbContext.Database.Migrate();

var apiKey = configuration.GetSection("TelegramBot")["ApiKey"];
if (string.IsNullOrEmpty(apiKey))
{
    Console.WriteLine("Telegram API Key could not be read or is empty in app-settings.json file");
    return;
}

var bot = new Bot(apiKey, dbContext);
bot.Start();

Console.WriteLine("Press any key to exit");
Console.ReadKey();