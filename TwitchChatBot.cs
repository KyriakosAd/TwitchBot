using System;
using System.IO;
using System.Threading.Tasks;
using TwitchLib.Client;
using TwitchLib.Client.Events;
using TwitchLib.Client.Models;
using TwitchLib.Communication.Clients;
using TwitchLib.Communication.Models;

namespace TwitchBot
{
    internal class TwitchChatBot
    {
        private readonly ConnectionCredentials credentials = new ConnectionCredentials(TwitchInfo.BotUsername, TwitchInfo.BotToken);
        private TwitchClient client;

        internal async Task Connect()
        {
            Console.WriteLine("Connected!");

            ClientOptions clientOptions = new ClientOptions
            {
                MessagesAllowedInPeriod = 7500,
                ThrottlingPeriod = TimeSpan.FromSeconds(30)
            };
            WebSocketClient customClient = new WebSocketClient(clientOptions);

            client = new TwitchClient(customClient);
            client.Initialize(credentials, autoReListenOnExceptions: false);

            client.OnConnected += Client_OnConnected;
            client.OnLog += Client_OnLog;
            client.OnConnectionError += Client_OnConnectionError;
            client.OnChatCommandReceived += Client_OnChatCommandReceived;
            client.OnJoinedChannel += Client_OnJoinedChannel;
            client.AddChatCommandIdentifier('^');
            client.RemoveChatCommandIdentifier('!');
            client.Connect();
        }

        private void Client_OnConnected(object sender, OnConnectedArgs e)
        {
            foreach (string channel in TwitchInfo.Channels)
                client.JoinChannel(channel);
        }

        private void Client_OnJoinedChannel(object sender, OnJoinedChannelArgs e)
        {
            client.SendMessage(e.Channel, "Connected!");
        }

        private async void Client_OnChatCommandReceived(object sender, OnChatCommandReceivedArgs e)
        {
            if (!Commands.CommandDictionary.TryGetValue(e.Command.CommandText, out Commands.CommandDelegate commandDelegate)) return;
            await commandDelegate(client, e.Command);
        }

        private static void Client_OnConnectionError(object sender, OnConnectionErrorArgs e)
        {
            Console.WriteLine($"Error!! {e.Error}");
        }

        private static void Client_OnLog(object sender, OnLogArgs ex)
        {
            FileStream ostrm;
            StreamWriter writer;
            try
            {
                ostrm = new FileStream("./Redirect.txt", FileMode.Append, FileAccess.Write);
                writer = new StreamWriter(ostrm);
            }
            catch (Exception e)
            {
                Console.WriteLine("Cannot open Redirect.txt for writing.");
                Console.WriteLine(e.Message);
                return;
            }

            string data = $"{ex.Data}";
            int index = data.IndexOf("user-type=", StringComparison.Ordinal);
            if (index != -1)
            {
                string result = data.Substring(index).Substring(data.IndexOf(':', index) - index + 1);
                if (result.Contains("PRIVMSG"))
                {
                    string user = result.Substring(0, result.IndexOf('!'));
                    string channel = result.Substring(0, result.IndexOf(':') - 1).Substring(result.IndexOf('#'));
                    string message = result.Substring(result.IndexOf(':') + 1);

                    writer.WriteLine($"{channel} {user}: {message}\n");
                    Console.WriteLine($"{channel} {user}: {message}\n");
                }
            }
            writer.Close();
            ostrm.Close();
        }
    }
}