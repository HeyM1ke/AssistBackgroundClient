using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DiscordRPC;
using ValNet;

namespace AssistBackgroundClient.Discord
{
    internal class DiscordPresence
    {
        #region DiscordSettings
        private const string _DISCORDAPPID = "925134832453943336";
        private static RichPresence currentPresence;
        private static DiscordRpcClient client = new (_DISCORDAPPID);


        #endregion
        public static readonly Button[] clientButtons =
        {
            new() {Label = "Download Assist", Url = "https://github.com/Rumblemike/Assist/releases/latest/download/AssistSetup.exe"},
            new() {Label = "Join the Discord!", Url = "https://discord.gg/B43EndmEgW"}
        };

        public static ValorantPresence ValorantPresence;


        public static void StartPresence()
        {
            ValorantPresence = new ValorantPresence();
            StartDiscordClient();
        }

        private static void StartDiscordClient()
        {
            currentPresence = new RichPresence
            {
                Buttons = clientButtons,
                Assets = new Assets()
                {
                    LargeImageKey = "default",
                    LargeImageText = "Powered By Assist"
                },
                Details = "Lobby",
                Party = new Party()
                {
                    Max = 5,
                    Size = 1
                },
                Secrets = null,
                State = "VALORANT"
            };

            client.OnReady += (_,args) => Console.WriteLine(args.User.Username + "Started Discord RPC");
            client.SetPresence(currentPresence);
            client.Initialize();
        }

        public static void UpdatePresence(RichPresence data)
        {
            // Go through Data
            currentPresence = data;

            // Send Data to Discord
            client.SetPresence(currentPresence);
            client.Invoke();
        }


    }
}
