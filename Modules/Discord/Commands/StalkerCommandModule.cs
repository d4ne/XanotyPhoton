using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Photon.Realtime;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace XanotyPhoton.Modules.Discord.Commands
{
    public class StalkerCommandModule : ModuleBase<SocketCommandContext>
    {
        [Command("users")]
        public async Task ListUsers()
        {
            try
            {
                var User = Context.User as SocketGuildUser;
                var role = Context.Guild.Roles.FirstOrDefault(x => x.Name == "Control");

                if (Context.Channel.Id.ToString() == "908754378926751764")
                {
                    if (User.Roles.Contains(role))
                    {
                        foreach (Player player in PhotonClient.playersInRoom.Values)
                        {
                            var embed = new EmbedBuilder()
                                .WithTitle($"{Context.User.Username}#{Context.User.Discriminator}")
                                .AddField("Username", Extensions.GetUsername(player), true)
                                .Build();

                            await Context.Channel.SendMessageAsync(embed: embed);
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
