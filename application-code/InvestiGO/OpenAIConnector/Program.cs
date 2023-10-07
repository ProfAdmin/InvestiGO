using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using OpenAIConnector.Data;
using OpenAIConnector.Services;

var configuration = new ConfigurationBuilder()
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .Build();

var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();
optionsBuilder.UseSqlServer(configuration.GetConnectionString("DefaultConnection"));

using var dbContext = new AppDbContext(optionsBuilder.Options);

var apiKey = configuration.GetSection("OpenAi")["ApiKey"];
if (string.IsNullOrEmpty(apiKey))
{
    Console.WriteLine("Open AI API Key could not be read or is empty in app-settings.json file");
    return;
}

var messageService = new MessageService(dbContext);
var groupService = new GroupService(dbContext);

var group = await groupService.GetFirsGroupAsync();

if (group == null || !group.IsActive)
{
    Console.WriteLine("No active group was found in the database! Please use the /register command from the bot first.");
    return;
}

var messages = await messageService.GetMessagesAsync(group.ChatId);

var openAiService = new OpenAIService(apiKey);
var summary = await openAiService.GetSummary(messages);

// Output the summary
Console.WriteLine(summary);

Console.WriteLine("Press any key to exit");
Console.ReadKey();