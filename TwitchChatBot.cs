using System;
using System.Data;
using System.Data.SQLite;
using System.Threading.Tasks;
using Dapper;
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
            string data = $"{ex.Data}";

            int index = data.IndexOf("user-type=", StringComparison.Ordinal);
            if (index == -1) return;

            string result = data[index..][(data.IndexOf(':', index) - index + 1)..];
            if (!result.Contains("PRIVMSG")) return;

            string user = result[..result.IndexOf('!')];
            string channel = result[..(result.IndexOf(':') - 1)][result.IndexOf('#')..];
            string message = result[(result.IndexOf(':') + 1)..];

            SQLiteConnectionStringBuilder connectionStringBuilder = new SQLiteConnectionStringBuilder
            {
                DataSource = "../../../BotDB.db",
                Version = 3
            };

            DynamicParameters parameters = new DynamicParameters();
            parameters.Add("@Channel", channel);
            parameters.Add("@User", user);
            parameters.Add("@Message", message);

            using (IDbConnection cnn = new SQLiteConnection(connectionStringBuilder.ConnectionString))
            {
                cnn.Execute("INSERT INTO Logs (Channel, User, Message) VALUES (@Channel, @User, @Message)", parameters);
            }

            Console.WriteLine($"{channel} {user}: {message}\n");
        }
    }
}