using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using AssistBackgroundClient.Objects;
using ValNet;
using WebSocketSharp;
using DiscordRPC;
using JsonSerializer = System.Text.Json.JsonSerializer;

namespace AssistBackgroundClient.Discord
{
    internal class ValorantPresence
    {
        public RiotUser currentUser;
        public static Presence userPresence;
        public static string userPrivate64;
        public static PrivateData userPrivateData;
        public static PrivateData prevUserPrivateData;

        private ulong timeStart = (ulong)DateTimeOffset.Now.ToUnixTimeSeconds();
        public ValorantPresence()
        {
            Console.WriteLine("Creating Presense");
            currentUser = new RiotUser();
            ConnectionLoop();
        }

        public async Task ConnectionLoop()
        {
            while (currentUser.tokenData.access is null)
            {
                Console.WriteLine("Authing");
                try
                {
                   await currentUser.Authentication.AuthenticateWithSocket();
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Error");
                    Console.WriteLine(ex.Message);
                }
                Console.WriteLine("Looking for Game");
                Thread.Sleep(10000);
            }



            Console.WriteLine("Connected to Val");
            currentUser.Authentication.userWebsocket.OnMessage += UserWebsocketOnOnMessage;
            currentUser.Authentication.userWebsocket.OnClose += UserWebsocketOnOnClose;
        }

        private void UserWebsocketOnOnClose(object? sender, CloseEventArgs e)
        {
            Environment.Exit(0);
        }

        private async void UserWebsocketOnOnMessage(object? sender, MessageEventArgs e)
        {
            await CalculateMessageAsync(e.Data);
        }


        #region Rich Presence Logic
        private async Task CalculateMessageAsync(string data)
        {
            if (!data.Contains("/chat/v4/presences") || !data.Contains(currentUser.UserData.sub))
                return;

           // Wizard Magic to make it work. 
            var stuff = JsonSerializer.Deserialize<List<object>>(data);
            var dataObj = stuff[stuff.Count - 1].ToString();
            var obj = JsonSerializer.Deserialize<PresencesV4DataObj>(dataObj);

            userPresence = obj.data.presences[0];

            userPrivate64 = userPresence.@private;

            Console.WriteLine(Encoding.UTF8.GetString(Convert.FromBase64String(userPrivate64)));

            await DetermineStatus();
        }
        private async Task DetermineStatus()
        {

            string non64 = Encoding.UTF8.GetString(Convert.FromBase64String(userPrivate64));

            userPrivateData = JsonSerializer.Deserialize<PrivateData>(non64);

            switch (userPrivateData.sessionLoopState)
            {
                case "MENUS":
                    await CreateMenuStatus();
                    break;
                case "PREGAME":
                    await CreatePregameStatus();
                    break;
                case "INGAME":
                    await CreateIngameStatus();
                    break;
                    
            }
        }
        private async Task CreateMenuStatus()
        {
            string details;
            ulong timeStart = 0;
            //Create Details
            switch (userPrivateData.partyState)
            {
                case "DEFAULT":
                    details = "In Lobby";
                    break;
                case "MATCHMAKING":
                    details = $"Queuing {char.ToUpper(userPrivateData.queueId[0]) + userPrivateData.queueId.Substring(1)}"; // magic woo, Capitalizes first letter.
                    break;
                default:
                    details = "In Lobby";
                    break;
            }

            DiscordRPC.Party.PrivacySetting privacy;

            if (userPrivateData.partyAccessibility.Equals("CLOSED"))
            {
                privacy = DiscordRPC.Party.PrivacySetting.Private;
            }
            else
            {
                privacy = DiscordRPC.Party.PrivacySetting.Public;
            }



            var newPres = new RichPresence()
            {
                Assets = new Assets
                {
                    LargeImageKey = "default",
                    LargeImageText = "Powered By Assist"
                },
                Buttons = DiscordPresence.clientButtons,
                Details = details,
                State = "Party: ",
                Party = new DiscordRPC.Party()
                {
                    ID = userPrivateData.partyId,
                    Max = userPrivateData.maxPartySize,
                    Privacy = privacy,
                    Size = userPrivateData.partySize
                },
                Secrets = null,

            };

            DiscordPresence.UpdatePresence(newPres);


        }
        private async Task CreatePregameStatus()
        {
            DiscordRPC.Party.PrivacySetting privacy;

            if (userPrivateData.partyAccessibility.Equals("CLOSED"))
            {
                privacy = DiscordRPC.Party.PrivacySetting.Private;
            }
            else
            {
                privacy = DiscordRPC.Party.PrivacySetting.Public;
            }


            string state = "Agent Select";

            string mapName = await DetermineMapKey();

            string details;
            if (userPrivateData.partyState.Contains("CUSTOM_GAME"))
                details = "Custom Game";
            else
            {
                details = char.ToUpper(userPrivateData.queueId[0]) + userPrivateData.queueId.Substring(1);
            }


            var newPres = new RichPresence()
            {
                Assets = new Assets
                {
                    LargeImageKey = mapName.ToLower(),
                    LargeImageText = "Powered By Assist"
                },
                Buttons = DiscordPresence.clientButtons,
                Details = details,
                State = state,
                Party = new DiscordRPC.Party()
                {
                    ID = userPrivateData.partyId,
                    Max = userPrivateData.maxPartySize,
                    Privacy = privacy,
                    Size = userPrivateData.partySize
                },
                Secrets = null,

            };

            DiscordPresence.UpdatePresence(newPres);

        }
        private async Task CreateIngameStatus()
        {
            if(userPrivateData.partyState.Equals("MATCHMADE_GAME_STARTING"))
                return;

            DiscordRPC.Party.PrivacySetting privacy;

            if (userPrivateData.partyAccessibility.Equals("CLOSED"))
            {
                privacy = DiscordRPC.Party.PrivacySetting.Private;
            }
            else
            {
                privacy = DiscordRPC.Party.PrivacySetting.Public;
            }

            string state = $"{userPrivateData.partyOwnerMatchScoreAllyTeam} - {userPrivateData.partyOwnerMatchScoreEnemyTeam}";

            string details;
            if (userPrivateData.partyState.Contains("CUSTOM_GAME"))
                details = "Custom Game";
            else
            {
                details = char.ToUpper(userPrivateData.queueId[0]) + userPrivateData.queueId.Substring(1);
            }

            string mapName = await DetermineMapKey();

            var newPres = new RichPresence()
            {
                Assets = new Assets
                {
                    LargeImageKey = mapName.ToLower(),
                    LargeImageText = "Powered By Assist"
                },
                Buttons = DiscordPresence.clientButtons,
                Details = details,
                State = state,
                Party = new DiscordRPC.Party()
                {
                    ID = userPrivateData.partyId,
                    Max = userPrivateData.maxPartySize,
                    Privacy = privacy,
                    Size = userPrivateData.partySize
                },
                Secrets = null,
                Timestamps = null

            };

            DiscordPresence.UpdatePresence(newPres);

        }
        private bool DetermineTimeReset()
        {
            if (prevUserPrivateData is null)
                return true;

            if (prevUserPrivateData.sessionLoopState == userPrivateData.sessionLoopState &&
                prevUserPrivateData.partyState == userPrivateData.partyState)
                return false;
            return true;
        }
        private async Task<string> DetermineMapKey()
        {
            switch (userPrivateData.matchMap)
            {
                case "/Game/Maps/Ascent/Ascent":
                    return "Ascent";
                case "/Game/Maps/Bonsai/Bonsai":
                    return "Split";
                case "/Game/Maps/Canyon/Canyon":
                    return "Fracture";
                case "/Game/Maps/Duality/Duality":
                    return "Bind";
                case "/Game/Maps/Foxtrot/Foxtrot":
                    return "Breeze";
                case "/Game/Maps/Triad/Triad":
                    return "Haven";
                case "/Game/Maps/Port/Port":
                    return "Icebox";
                case "/Game/Maps/Poveglia/Range":
                    return "Range";
                default:
                    return "Unknown";
            }

        }
        #endregion

    }



}
