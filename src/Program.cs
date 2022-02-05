using System;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using AssistBackgroundClient.Discord;

namespace AssistBackgroundClient
{
    internal class Program
    {
        [DllImport("Kernel32.dll")]
        private static extern IntPtr GetConsoleWindow();

        [DllImport("User32.dll")]
        private static extern bool ShowWindow(IntPtr hWnd, int cmdShow);

        static void Main(string[] args)
        {
            Console.WriteLine("Starting AssistBackgroundClient");
            IntPtr hWnd = GetConsoleWindow();
            if (hWnd != IntPtr.Zero)
            {
                ShowWindow(hWnd, 0);
            }

            MainAsync().GetAwaiter().GetResult();
        }

        static async Task MainAsync()
        {
            DiscordPresence.StartPresence();

            await Task.Delay(-1);
        }
    }
}
