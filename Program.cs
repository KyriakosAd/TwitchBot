using System;
using System.Threading.Tasks;

namespace TwitchBot
{
    internal static class Program
    {
        private static async Task Main()
        {
            Console.OutputEncoding = System.Text.Encoding.Unicode;

            TwitchChatBot bot = new TwitchChatBot();
            await bot.Connect();
            Console.ReadLine();
        }
    }
}