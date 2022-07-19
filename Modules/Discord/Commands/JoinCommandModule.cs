using Discord.Commands;
using Discord.WebSocket;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using XanotyPhoton.Modules.Configuration;

namespace XanotyPhoton.Modules.Discord.Commands
{
    public class JoinCommandModule : ModuleBase<SocketCommandContext>
    {
        [Command("join")]
        public async Task JoinWorld(string world)
        {
            try
            {
                var User = Context.User as SocketGuildUser;
                var role = Context.Guild.Roles.FirstOrDefault(x => x.Name == "Control");

                if (Context.Channel.Id.ToString() == "908754378926751764")
                {
                    if (User.Roles.Contains(role))
                    {
                        string rominstance = world;

                        if (rominstance.Contains("eu"))
                        {
                            Config.region = "eu";
                        }
                        else if (rominstance.Contains("jp"))
                        {
                            Config.region = "jp";
                        }

                        try
                        {
                            VRBot bot = new VRBot(Config.botAccount);

                            Thread.Sleep(3000);

                            bot.JoinRoom(rominstance);

                            while (!bot.photonclient.IsInstantiated)
                            {
                                Thread.Sleep(500);
                            }
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine(ex.Message);
                        }
                    }
                    else
                    {
                        await Context.Channel.SendMessageAsync("Error: Missing permission...");
                    }
                }
                else
                {
                    await Context.Channel.SendMessageAsync("Error: Use the bot-controller channel...");
                }
            }
            catch (Exception ex)
            {
                await Context.Channel.SendMessageAsync($"Exception: {ex.Message}");
            }
        }
    }
}
