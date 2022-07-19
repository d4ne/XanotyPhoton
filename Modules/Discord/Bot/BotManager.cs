using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Reflection;
using System.Threading.Tasks;
using XanotyPhoton.Modules.Configuration;
using XanotyPhoton.Modules.Discord.Commands;

namespace XanotyPhoton.Modules.Discord.Bot
{
    public class BotManager
    {
        public static DiscordSocketClient BotClient;
        public static CommandService Commands;
        public static IServiceProvider ServiceProvider;

        public async Task RunBot()
        {
            BotClient = new DiscordSocketClient();
            Commands = new CommandService();
            ServiceProvider = ConfigureServices();

            await BotClient.LoginAsync(TokenType.Bot, Config.token);
            await BotClient.StartAsync();

            BotClient.Log += BotLogging;
            BotClient.Ready += BotReady;

            await Task.Delay(-1);
        }

        public Task BotLogging(LogMessage message)
        {
            Console.WriteLine($"Bot: {message}");

            return Task.CompletedTask;
        }

        public async Task BotReady()
        {
            await Commands.AddModulesAsync(Assembly.GetEntryAssembly(), ServiceProvider);

            await BotClient.SetGameAsync("VRChat");
            BotClient.MessageReceived += BotMessage;

            var chnl = BotClient.GetChannel(908740573127663617) as IMessageChannel;
            await chnl.SendMessageAsync("Bot started...");
        }

        public async Task BotMessage(SocketMessage arg)
        {
            SocketUserMessage message = arg as SocketUserMessage;

            int commandPosition = 0;

            if (message.HasStringPrefix(Config.prefix, ref commandPosition))
            {
                SocketCommandContext context = new SocketCommandContext(BotClient, message);
                IResult result = await Commands.ExecuteAsync(context, commandPosition, ServiceProvider);

                if (!result.IsSuccess)
                {
                    Console.WriteLine($"Error: {result.ErrorReason}");
                }
            }
        }

        public IServiceProvider ConfigureServices()
        {
            return new ServiceCollection()
                .AddSingleton<JoinCommandModule>()
                .AddSingleton<StalkerCommandModule>()
                .BuildServiceProvider();
        }
    }
}
