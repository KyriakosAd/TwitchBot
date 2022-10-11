using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using TwitchLib.Client;
using TwitchLib.Client.Models;

namespace TwitchBot
{
    internal static class Commands
    {
        internal delegate Task CommandDelegate(TwitchClient client, ChatCommand command);
        internal static Dictionary<string, CommandDelegate> CommandDictionary { get; }

        private static readonly Dictionary<string, DateTime> CookieTimes = new Dictionary<string, DateTime>();

        static Commands()
        {
            CommandDictionary = new Dictionary<string, CommandDelegate>();

            MethodInfo[] methods = typeof(Commands).GetMethods(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);

            foreach (MethodInfo method in methods)
            {
                CommandAttribute attribute = method.GetCustomAttribute<CommandAttribute>();

                if (attribute == null)
                    continue;

                CommandDictionary.Add(attribute.Command, (CommandDelegate)method.CreateDelegate(typeof(CommandDelegate)));
            }
        }

        [Command("slots")]
        internal static async Task Slots(TwitchClient client, ChatCommand command)
        {
            StringBuilder messageBuilder = new StringBuilder();
            messageBuilder.Append(command.ChatMessage.Username);

            List<string> emotesMessage = await Apis.GetAllEmotes(command);

            switch (emotesMessage.Count)
            {
                case 0:
                    messageBuilder.Append(", No emotes in message!");
                    break;

                case 1:
                    messageBuilder.Append(", 2 more emotes needed.");
                    break;

                case 2:
                    messageBuilder.Append(", 1 more emote needed.");
                    break;

                case 3:
                    Random random = new Random();
                    int index1 = random.Next(emotesMessage.Count);
                    int index2 = random.Next(emotesMessage.Count);
                    int index3 = random.Next(emotesMessage.Count);

                    if (emotesMessage[index1] == emotesMessage[index2] && emotesMessage[index1] == emotesMessage[index3])
                    {
                        messageBuilder.Append(", won! [ ").Append(emotesMessage[index1]).Append(' ').Append(emotesMessage[index2]).Append(' ').Append(emotesMessage[index3]).Append(" ]");
                    }
                    else
                    {
                        messageBuilder.Append(", [ ").Append(emotesMessage[index1]).Append(' ').Append(emotesMessage[index2]).Append(' ').Append(emotesMessage[index3]).Append(" ]");
                    }
                    break;

                default:
                    messageBuilder.Append(", Too many emotes, try 3.");
                    break;
            }

            client.SendMessage(command.ChatMessage.Channel, messageBuilder.ToString());
            Thread.Sleep(1250);
        }

        [Command("pyramid")]
        internal static async Task Pyramid(TwitchClient client, ChatCommand command)
        {
            StringBuilder messageBuilder = new StringBuilder();

            List<string> emotesMessage = await Apis.GetAllEmotes(command);

            switch (emotesMessage.Count)
            {
                case 0:
                    messageBuilder.Append(command.ChatMessage.Username).Append(", no emotes in message.");

                    client.SendMessage(command.ChatMessage.Channel, messageBuilder.ToString());
                    Thread.Sleep(1250);
                    break;

                case 1:
                    string sizeCheck = Regex.Match(command.ArgumentsAsString, @"\d+").Value;

                    if (sizeCheck.Length == 0)
                    {
                        messageBuilder.Append(command.ChatMessage.Username).Append(", no number provided.");

                        client.SendMessage(command.ChatMessage.Channel, messageBuilder.ToString());
                        Thread.Sleep(1250);
                        break;
                    }

                    int size = int.Parse(sizeCheck);

                    if (size > 10)
                    {
                        messageBuilder.Append(command.ChatMessage.Username).Append(", number too big try under 10.");

                        client.SendMessage(command.ChatMessage.Channel, messageBuilder.ToString());
                        Thread.Sleep(1250);
                        break;
                    }

                    for (int i = 0; i < size; i++)
                    {
                        messageBuilder.Append(emotesMessage[0]).Append(' ');

                        client.SendMessage(command.ChatMessage.Channel, messageBuilder.ToString());
                        Thread.Sleep(1250);
                    }
                    for (int i = size; i > 0; i--)
                    {
                        messageBuilder.Remove(0, emotesMessage[0].Length + 1);

                        client.SendMessage(command.ChatMessage.Channel, messageBuilder.ToString());
                        Thread.Sleep(1250);
                    }
                    break;

                default:
                    messageBuilder.Append(command.ChatMessage.Username).Append(", choose just 1 emote.");

                    client.SendMessage(command.ChatMessage.Channel, messageBuilder.ToString());
                    Thread.Sleep(1250);
                    break;
                
            }
        }

        [Command("google")]
        internal static async Task Google(TwitchClient client, ChatCommand command)
        {
            StringBuilder messageBuilder = new StringBuilder();
            messageBuilder.Append(command.ChatMessage.Username);

            if (command.ArgumentsAsList.Count == 0)
            {
                messageBuilder.Append(", Google what?");
            }
            else
            {
                string search = command.ArgumentsAsString.Replace("&", " and ");
                string result = await Apis.GetGoogle(search);

                if (result == null)
                {
                    messageBuilder.Append(", No result found.");
                }
                else
                {
                    messageBuilder.Append(", ").Append(result);
                }
            }

            client.SendMessage(command.ChatMessage.Channel, messageBuilder.ToString());
            Thread.Sleep(1250);
        }

        [Command("image")]
        internal static async Task Image(TwitchClient client, ChatCommand command)
        {
            StringBuilder messageBuilder = new StringBuilder();
            messageBuilder.Append(command.ChatMessage.Username);

            if (command.ArgumentsAsList.Count == 0)
            {
                messageBuilder.Append(", Image of what?");
            }
            else
            {
                string search = command.ArgumentsAsString.Replace("&", " and ");
                string result = await Apis.GetImage(search);

                if (result == null)
                {
                    messageBuilder.Append(", No result found.");
                }
                else
                {
                    messageBuilder.Append(", ").Append(result);
                }
            }

            client.SendMessage(command.ChatMessage.Channel, messageBuilder.ToString());
            Thread.Sleep(1250);
        }

        [Command("snippet")]
        internal static async Task Snippet(TwitchClient client, ChatCommand command)
        {
            StringBuilder messageBuilder = new StringBuilder();
            messageBuilder.Append(command.ChatMessage.Username);

            if (command.ArgumentsAsList.Count == 0)
            {
                messageBuilder.Append(", Snippet of what?");
            }
            else
            {
                string search = command.ArgumentsAsString.Replace("&", " and ");
                string result = await Apis.GetSnippet(search);

                if (result == null)
                {
                    messageBuilder.Append(", No result found.");
                }
                else
                {
                    messageBuilder.Append(", ").Append(result);
                }
            }

            client.SendMessage(command.ChatMessage.Channel, messageBuilder.ToString());
            Thread.Sleep(1250);
        }

        [Command("video")]
        internal static async Task Video(TwitchClient client, ChatCommand command)
        {
            StringBuilder messageBuilder = new StringBuilder();
            messageBuilder.Append(command.ChatMessage.Username);

            if (command.ArgumentsAsList.Count == 0)
            {
                messageBuilder.Append(", Video of what?");
            }
            else
            {
                string search = command.ArgumentsAsString.Replace("&", " and ");
                List<string> result = await Apis.GetVideo(search);

                if (!result.Any())
                {
                    messageBuilder.Append(", No result found.");
                }
                else
                {
                    string titleDecoded = WebUtility.HtmlDecode(result[1]);
                    messageBuilder.Append(", ").Append(titleDecoded).Append(" https://www.youtube.com/watch?v=").Append(result[0]);
                }
            }

            client.SendMessage(command.ChatMessage.Channel, messageBuilder.ToString());
            Thread.Sleep(1250);
        }

        [Command("latest")]
        internal static async Task Latest(TwitchClient client, ChatCommand command)
        {
            StringBuilder messageBuilder = new StringBuilder();
            messageBuilder.Append(command.ChatMessage.Username);

            if (command.ArgumentsAsList.Count == 0)
            {
                messageBuilder.Append(", latest video of who?");
            }
            else
            {
                string search = command.ArgumentsAsString.Replace("&", " and ");
                List<string> result = await Apis.GetLatestVideo(search);

                if (!result.Any())
                {
                    messageBuilder.Append(", No result found.");
                }
                else
                {
                    string titleDecoded = WebUtility.HtmlDecode(result[1]);
                    messageBuilder.Append(", ").Append(titleDecoded).Append(" https://www.youtube.com/watch?v=").Append(result[0]);
                }
            }

            client.SendMessage(command.ChatMessage.Channel, messageBuilder.ToString());
            Thread.Sleep(1250);
        }

        [Command("cookie")]
        internal static async Task Cookie(TwitchClient client, ChatCommand command)
        {
            StringBuilder messageBuilder = new StringBuilder();
            messageBuilder.Append(command.ChatMessage.Username);

            DateTime lastCookie = DateTime.MinValue;

            string user = command.ChatMessage.Username;

            if (CookieTimes.ContainsKey(user))
            {
                lastCookie = CookieTimes[user];
            }

            TimeSpan sinceLastCookie = DateTime.Now - lastCookie;
            TimeSpan cookieDelay = TimeSpan.FromMinutes(10);

            if (sinceLastCookie >= cookieDelay)
            {
                string cookie = await Apis.GetCookie();

                messageBuilder.Append(", ").Append(cookie);

                CookieTimes[user] = DateTime.Now;
            }
            else
            {
                TimeSpan remainingTime = cookieDelay - sinceLastCookie;
                messageBuilder.Append(", No cookie left. New cookie in ").AppendFormat("{0:mm'm, 'ss's'}", remainingTime).Append('.');
            }

            client.SendMessage(command.ChatMessage.Channel, messageBuilder.ToString());
            Thread.Sleep(1250);
        }

        [Command("cat")]
        internal static async Task Cat(TwitchClient client, ChatCommand command)
        {
            StringBuilder messageBuilder = new StringBuilder();
            messageBuilder.Append(command.ChatMessage.Username);

            string[] cats = {
                "abyssinian", "aegean", "curl", "american", "wirehair", "mist", "base",
                "bambino", "bengal", "birman", "bombay", "longhair", "british", "burmese",
                "burmilla", "spangled", "chantilly", "chartreux", "chausie", "cheetoh",
                "colorpoint", "cornish", "cymric", "cyprus", "devon", "donskoy", "dragon",
                "mau", "havana", "himalayan", "bobtail", "javanese", "khao", "korat", "kurilian",
                "laperm", "maine", "malayan", "manx", "munchkin", "nebelung", "forest", "ocicat",
                "oriental", "persian", "pixie", "ragamuffin", "ragdoll", "russian", "savannah",
                "fold", "selkirk", "siamese", "siberian", "singapura", "snowshoe", "somali",
                "sphynx", "tonkinese", "toyger", "angora", "van", "york"
            };

            string[] catIds = {
                "abys", "aege", "acur", "asho", "awir", "amis", "bali", "bamb", "beng", "birm",
                "bomb", "bslo", "bsho", "bure", "buri", "cspa", "ctif", "char", "chau", "chee",
                "csho", "crex", "cymr", "cypr", "drex", "dons", "lihu", "emau", "hbro", "hima",
                "jbob", "java", "khao", "kora", "kuri", "lape", "mcoo", "mala", "manx", "munc",
                "nebe", "norw", "ocic", "orie", "pers", "pixi", "raga", "ragd", "rblu", "sava",
                "sfol", "srex", "siam", "sibe", "sing", "snow", "soma", "sphy", "tonk", "toyg",
                "tang", "tvan", "ycho"
            };

            int count = cats.Intersect(command.ArgumentsAsList, StringComparer.OrdinalIgnoreCase).Count();

            string cat;
            switch (count)
            {
                case 0:
                    cat = await Apis.GetCat();

                    messageBuilder.Append(", ").Append(cat).Append(" Random Breed");
                    break;

                case 1:
                    string search = cats.Intersect(command.ArgumentsAsList, StringComparer.OrdinalIgnoreCase).First();
                    int index = Array.FindIndex(cats, x => x == $"{search}");
                    
                    cat = await Apis.GetCat(catIds[index]);

                    messageBuilder.Append(", ").Append(cat).Append(' ').Append(search);
                    break;

                default:
                    messageBuilder.Append(", ").Append(count).Append(" breeds selected, choose just one.");
                    break;
            }

            client.SendMessage(command.ChatMessage.Channel, messageBuilder.ToString());
            Thread.Sleep(1250);
        }

        [Command("dog")]
        internal static async Task Dog(TwitchClient client, ChatCommand command)
        {
            StringBuilder messageBuilder = new StringBuilder();
            messageBuilder.Append(command.ChatMessage.Username);

            string[] dogs = {
                "affenpinscher", "african", "airedale", "akita", "appenzeller", "australian", "basenji", "beagle",
                "bluetick", "borzoi", "bouvier", "boxer", "brabancon", "briard", "buhund", "bulldog", "bullterrier",
                "cairn", "cattledog", "chihuahua", "chow", "clumber", "cockapoo", "collie", "coonhound", "corgi",
                "cotondetulear", "dachshund", "dalmatian", "dane", "deerhound", "dhole", "dingo", "doberman",
                "elkhound", "entlebucher", "eskimo", "finnish", "frise", "germanshepherd", "greyhound", "groenendael",
                "havanese", "hound", "husky", "keeshond", "kelpie", "komondor", "kuvasz", "labrador", "leonberg", "lhasa",
                "malamute", "malinois", "maltese", "mastiff", "mexicanhairless", "mix", "mountain", "newfoundland",
                "otterhound", "ovcharka", "papillon", "pekinese", "pembroke", "pinscher", "pitbull", "pointer", "pomeranian",
                "poodle", "pug", "puggle", "pyrenees", "redbone", "retriever", "ridgeback", "rottweiler", "saluki",
                "samoyed", "schipperke", "schnauzer", "setter", "sheepdog", "shiba", "shihtzu", "spaniel", "springer",
                "stbernard", "terrier", "vizsla", "waterdog", "weimaraner", "whippet", "wolfhound"
            };

            int count = dogs.Intersect(command.ArgumentsAsList, StringComparer.OrdinalIgnoreCase).Count();

            string dog;
            switch (count)
            {
                case 0:
                    dog = await Apis.GetDog();

                    messageBuilder.Append(", ").Append(dog).Append(" Random Breed");
                    break;
                
                case 1:
                    string search = dogs.Intersect(command.ArgumentsAsList, StringComparer.OrdinalIgnoreCase).First();

                    dog = await Apis.GetDog(search);

                    messageBuilder.Append(", ").Append(dog).Append(' ').Append(search);
                    break;

                default:
                    messageBuilder.Append(", ").Append(count).Append(" breeds selected, choose just one.");
                    break;
            }

            client.SendMessage(command.ChatMessage.Channel, messageBuilder.ToString());
            Thread.Sleep(1250);
        }

        [Command("ping")]
        internal static async Task Ping(TwitchClient client, ChatCommand command)
        {
            StringBuilder messageBuilder = new StringBuilder();
            messageBuilder.Append(command.ChatMessage.Username);

            using (System.Diagnostics.Process current = System.Diagnostics.Process.GetCurrentProcess())
            {
                DateTime now = DateTime.Now;
                long diff1 = (now - current.StartTime).Hours;
                long diff2 = (now - current.StartTime).Minutes;
                long diff3 = (now - current.StartTime).Seconds;

                string hours;
                if (diff1 == 1) hours = "1 hour";
                else if (diff1 == 0) hours = "";
                else hours = diff1 + " hours";

                string minutes = "";
                if (diff2 == 1 && diff1 >= 1 && diff3 >= 1) minutes = ", 1 minute";
                else if (diff1 == 0 && diff2 == 1) minutes = "1 minute";
                else if (diff1 == 0 && diff2 > 1) minutes = diff2 + " minutes";
                else if (diff2 == 0) minutes = "";
                else if (diff2 > 1 && diff1 >= 1 && diff3 >= 1) minutes = ", " + diff2 + " minutes";
                else if (diff2 == 1 && diff1 >= 1 && diff3 == 0) minutes = " and 1 minute";
                else if (diff2 > 1 && diff1 >= 1 && diff3 == 0) minutes = " and " + diff2 + " minutes";

                string seconds = "";
                if ((diff2 >= 1 && diff3 == 1) || (diff1 >= 1 && diff2 == 0 && diff3 == 1)) seconds = " and 1 second";
                else if ((diff2 >= 1 && diff3 > 1) || (diff1 >= 1 && diff2 == 0 && diff3 > 1)) seconds = " and " + diff3 + " seconds";
                else if (diff1 == 0 && diff2 == 0 && diff3 == 1) seconds = "1 second";
                else if (diff1 == 0 && diff2 == 0 && diff3 > 1) seconds = diff3 + " seconds";
                else if (diff3 == 0) seconds = "";

                string result = "Bot runtime: " + hours + minutes + seconds + ".";

                if (diff1 == 0 && diff2 == 0 && diff3 == 0)
                {
                    messageBuilder.Append(", Bot down?");
                }
                else
                {
                    long ram = current.PrivateMemorySize64;
                    messageBuilder.Append(", Pong! ").Append(result).Append(" RAM Usage: ").AppendFormat("{0:F}", ram / (1024.0 * 1024.0)).Append(" MB");
                }
            }

            client.SendMessage(command.ChatMessage.Channel, messageBuilder.ToString());
            Thread.Sleep(1250);
        }

        [Command("%")]
        internal static async Task RandomPercentage(TwitchClient client, ChatCommand command)
        {
            StringBuilder messageBuilder = new StringBuilder();
            messageBuilder.Append(command.ChatMessage.Username);

            Random rdm = new Random();
            int number = rdm.Next(0, 100);

            messageBuilder.Append(", ").Append(number).Append('%');

            client.SendMessage(command.ChatMessage.Channel, messageBuilder.ToString());
            Thread.Sleep(1250);
        }

        [Command("say")]
        internal static async Task Say(TwitchClient client, ChatCommand command)
        {
            StringBuilder messageBuilder = new StringBuilder();

            if (TwitchInfo.AdminUsernames.Contains(command.ChatMessage.Username))
            {
                if (command.ArgumentsAsList.Count == 0)
                {
                    messageBuilder.Append(command.ChatMessage.Username).Append(", Say what?");
                }
                else
                {
                    messageBuilder.Append(command.ArgumentsAsString);
                }
            }
            else
            {
                messageBuilder.Append(", You don't have permission to use this command.");
            }

            client.SendMessage(command.ChatMessage.Channel, messageBuilder.ToString());
            Thread.Sleep(1250);
        }

        [Command("help")]
        internal static async Task Help(TwitchClient client, ChatCommand command)
        {
            StringBuilder messageBuilder = new StringBuilder();
            messageBuilder.Append(command.ChatMessage.Username);

            messageBuilder.Append(", Bot made in C#, to see commands available try ^commands");

            client.SendMessage(command.ChatMessage.Channel, messageBuilder.ToString());
            Thread.Sleep(1250);
        }

        [Command("commands")]
        internal static async Task CommandsList(TwitchClient client, ChatCommand command)
        {
            StringBuilder messageBuilder = new StringBuilder();
            messageBuilder.Append(command.ChatMessage.Username);

            messageBuilder.Append(", Commands available: ");
            string commands = string.Join(", ", CommandDictionary.Keys.OrderBy(x => x));
            messageBuilder.Append(commands);

            client.SendMessage(command.ChatMessage.Channel, messageBuilder.ToString());
            Thread.Sleep(1250);
        }
    }

    [AttributeUsage(AttributeTargets.All)]
    internal class CommandAttribute : Attribute
    {
        internal string Command { get; }

        internal CommandAttribute(string command)
        {
            Command = command;
        }
    }
}