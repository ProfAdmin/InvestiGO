using TelegramBot;
using TelegramBot.Data;

using var dbContext = new AppDbContext();
var bot = new Bot("6571967465:AAFXuqGMd6b3a49PZg_PrpBF4i0-0qtgvfI", dbContext, -1001839414047);
bot.Start();

Console.WriteLine("Press any key to exit");
Console.ReadKey();