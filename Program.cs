using XanotyPhoton.Modules.Discord.Bot;

namespace XanotyPhoton
{
    public class Program
    {
        private static void Main(string[] args)
        {
            BotManager manager = new BotManager();
            manager.RunBot().GetAwaiter().GetResult();
        }
    }
}
