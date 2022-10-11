using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using TwitchLib.Client.Models;

namespace TwitchBot
{
    internal static class Apis
    {
        private static readonly ServiceProvider ServiceProvider = new ServiceCollection().AddHttpClient().BuildServiceProvider();

        private static readonly IHttpClientFactory HttpClientFactory = ServiceProvider.GetService<IHttpClientFactory>();

        private static async Task<JToken> GetData(string baseUrl)
        {
            HttpClient httpClient = HttpClientFactory.CreateClient();
            HttpResponseMessage response = await httpClient.GetAsync(baseUrl);

            if (!response.IsSuccessStatusCode)
            {
                Console.WriteLine((int)response.StatusCode);
                Console.WriteLine(response.ReasonPhrase);
                return null;
            }

            string data = await response.Content.ReadAsStringAsync();
            JToken dataJson = JToken.Parse(data);

            return dataJson;
        }

        internal static async Task<string> GetCookie()
        {
            const string baseUrl = "https://www.affirmations.dev/";

            JToken data = await GetData(baseUrl);

            return data?.SelectToken("affirmation")?.Value<string>();
        }

        internal static async Task<string> GetCat()
        {
            const string baseUrl = "https://api.thecatapi.com/v1/images/search";

            JToken data = await GetData(baseUrl);

            return data?.First?.SelectToken("url")?.Value<string>();
        }

        internal static async Task<string> GetCat(string search)
        {
            string baseUrl = $"https://api.thecatapi.com/v1/images/search?breed_ids={search}";

            JToken data = await GetData(baseUrl);

            return data?.First?.SelectToken("url")?.Value<string>();
        }

        internal static async Task<string> GetDog()
        {
            const string baseUrl = "https://dog.ceo/api/breeds/image/random";

            JToken data = await GetData(baseUrl);

            return data?.SelectToken("message")?.Value<string>();
        }

        internal static async Task<string> GetDog(string search)
        {
            string baseUrl = $"https://dog.ceo/api/breed/{search}/images/random";

            JToken data = await GetData(baseUrl);

            return data?.SelectToken("message")?.Value<string>();
        }

        internal static async Task<string> GetGoogle(string search)
        {
            string baseUrl = $"https://www.googleapis.com/customsearch/v1?key={TwitchInfo.GoogleKey}&safe=active&q={search}";

            JToken data = await GetData(baseUrl);

            return data?.SelectToken("items")?.First?.SelectToken("link")?.Value<string>();
        }

        internal static async Task<string> GetImage(string search)
        {
            string baseUrl = $"https://www.googleapis.com/customsearch/v1?key={TwitchInfo.GoogleKey}&safe=active&searchType=image&q={search}";

            JToken data = await GetData(baseUrl);

            return data?.SelectToken("items")?.First?.SelectToken("link")?.Value<string>();
        }

        internal static async Task<string> GetSnippet(string search)
        {
            string baseUrl = $"https://www.googleapis.com/customsearch/v1?key={TwitchInfo.GoogleKey}&safe=active&q={search}";

            JToken data = await GetData(baseUrl);

            return data?.SelectToken("items")?.First?.SelectToken("snippet")?.Value<string>();
        }

        internal static async Task<List<string>> GetVideo(string search)
        {
            string baseUrl = $"https://www.googleapis.com/youtube/v3/search?part=snippet&key={TwitchInfo.VideoKey}&safeSearch=moderate&type=video&q={search}";

            List<string> result = new List<string>();
            JToken data = await GetData(baseUrl);
            if (data == null)
            {
                return result;
            }

            result.Add(data.SelectToken("items")?.First?.SelectToken("id")?.SelectToken("videoId")?.Value<string>());
            result.Add(data.SelectToken("items")?.First?.SelectToken("snippet")?.SelectToken("title")?.Value<string>());
            result.RemoveAll(string.IsNullOrWhiteSpace);

            return result;
        }

        private static async Task<string> GetChannel(string search)
        {
            string baseUrl = $"https://www.googleapis.com/youtube/v3/search?part=snippet&maxResults=1&q={search}&type=channel&key={TwitchInfo.VideoKey}";

            JToken data = await GetData(baseUrl);

            return data?.SelectToken("items")?.First?.SelectToken("id")?.SelectToken("channelId")?.Value<string>();
        }

        internal static async Task<List<string>> GetLatestVideo(string search)
        {
            string channelId = await Apis.GetChannel(search);

            return await GetVideo($"&channelId={channelId}&order=date");
        }

        private static async Task<List<string>> GetBttvEmotesChannel(string channelId)
        {
            string baseUrl = $"https://api.betterttv.net/3/cached/users/twitch/{channelId}";

            List<string> result = new List<string>();
            JToken data = await GetData(baseUrl);
            if (data == null)
            {
                return result;
            }

            result.AddRange(data["channelEmotes"]?.Select(x => x["code"].Value<string>()) ?? Array.Empty<string>());
            result.AddRange(data["sharedEmotes"]?.Select(x => x["code"].Value<string>()) ?? Array.Empty<string>());

            return result;
        }

        private static async Task<List<string>> GetBttvEmotesGlobal()
        {
            const string baseUrl = "https://api.betterttv.net/3/cached/emotes/global";

            List<string> result = new List<string>();
            JToken data = await GetData(baseUrl);
            if (data == null)
            {
                return result;
            }

            result.AddRange(data.Select(x => x["code"].Value<string>()));

            return result;
        }

        private static async Task<List<string>> GetFfzEmotesChannel(string channelName)
        {
            string baseUrl = $"https://api.frankerfacez.com/v1/room/{channelName}";

            List<string> result = new List<string>();
            JToken data = await GetData(baseUrl);
            if (data == null)
            {
                return result;
            }

            result.AddRange(data.SelectToken("sets")?.First?.First?.SelectToken("emoticons")?.Select(x => x["name"].Value<string>()) ?? Array.Empty<string>());
            
            return result;
        }

        private static async Task<List<string>> GetFfzEmotesGlobal()
        {
            const string baseUrl = "https://api.frankerfacez.com/v1/set/global";

            List<string> result = new List<string>();
            JToken data = await GetData(baseUrl);
            if (data == null)
            {
                return result;
            }

            result.AddRange(data.SelectToken("sets")?.SelectToken("3")?.SelectToken("emoticons")?.Select(x => x["name"].Value<string>()) ?? Array.Empty<string>());
            result.AddRange(data.SelectToken("sets")?.SelectToken("4330")?.SelectToken("emoticons")?.Select(x => x["name"].Value<string>()) ?? Array.Empty<string>());
            
            return result;
        }

        private static async Task<List<string>> GetSeventvEmotesChannel(string channelId)
        {
            string baseUrl = $"https://7tv.io/v3/users/twitch/{channelId}";

            List<string> result = new List<string>();
            JToken data = await GetData(baseUrl);
            if (data == null)
            {
                return result;
            }

            result.AddRange(data.SelectToken("emotes")?.Select(x => x["name"].Value<string>()) ?? Array.Empty<string>());
            
            return result;
        }

        private static async Task<List<string>> GetSeventvEmotesGlobal()
        {
            const string baseUrl = "https://7tv.io/v3/emote-sets/global";

            List<string> result = new List<string>();
            JToken data = await GetData(baseUrl);
            if (data == null)
            {
                return result;
            }

            result.AddRange(data.SelectToken("emotes")?.Select(x => x["name"].Value<string>()) ?? Array.Empty<string>());

            return result;
        }

        internal static async Task<List<string>> GetAllEmotes(ChatCommand command)
        {
            List<string> bttvChannel = await Apis.GetBttvEmotesChannel(command.ChatMessage.RoomId);
            List<string> bttvGlobal = await Apis.GetBttvEmotesGlobal();
            List<string> ffzChannel = await Apis.GetFfzEmotesChannel(command.ChatMessage.Channel);
            List<string> ffzGlobal = await Apis.GetFfzEmotesGlobal();
            List<string> seventvChannel = await Apis.GetSeventvEmotesChannel(command.ChatMessage.RoomId);
            List<string> seventvGlobal = await Apis.GetSeventvEmotesGlobal();

            List<string> thirdPartyEmotes = bttvChannel.Union(bttvGlobal).Union(ffzChannel).Union(ffzGlobal).Union(seventvChannel).Union(seventvGlobal).ToList();

            List<string> twitchEmotesMessage = command.ChatMessage.EmoteSet.Emotes.ConvertAll(emote => emote.Name);
            List<string> thirdPartyEmotesMessage = thirdPartyEmotes.Intersect(command.ArgumentsAsList).ToList();

            List<string> emotesMessage = twitchEmotesMessage.Union(thirdPartyEmotesMessage).ToList();

            return emotesMessage;
        }
    }
}