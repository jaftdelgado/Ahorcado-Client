using AhorcadoClient.Model;
using System;

namespace AhorcadoClient.Utilities
{
    public static class CurrentSession
    {
        public static Player LoggedInPlayer { get; private set; }
        public static DateTime StartTime { get; set; }
        public static bool IsActive => LoggedInPlayer != null;

        public static void SetUser(Player player)
        {
            LoggedInPlayer = player;
            StartTime = DateTime.Now;
        }

        public static void LogOut()
        {
            LoggedInPlayer = null;
            StartTime = DateTime.MinValue;
        }
    }
}
