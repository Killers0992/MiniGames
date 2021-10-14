using MiniGames.Api;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniGames
{
    public static class MinigameSettings
    {
        public static int VotingTime { get; set; } = 40;
        public static int MaxPlayers { get; set; } = 4;
        public static bool IsVotingRunning { get; } = VotingStatus != VotingStatus.Ended;
        public static bool DisableEscape { get; set; } = false;
        public static bool Scp914Disabled { get; set; } = false;
        public static bool WarheadDisabled { get; set; } = false;
        public static bool MTFRespawnDisabled { get; set; } = false;
        public static bool TeslaDisabled { get; set; } = false;
        public static bool FriendlyFire
        {
            get
            {
                return ServerConsole.FriendlyFire;
            }
            set
            {
                ServerConsole.FriendlyFire = value;
                ServerConfigSynchronizer.Singleton.RefreshMainBools();
            }
        }
        public static int SelectedEventID { get; set; } = -1;
        public static VotingStatus VotingStatus { get; set; } = VotingStatus.WaitingForPlayers;


        public static void SetDefault()
        {
            VotingTime = 40;
            DisableEscape = false;
            Scp914Disabled = false;
            WarheadDisabled = false;
            MTFRespawnDisabled = false;
            TeslaDisabled = false;
            FriendlyFire = false;
            VotingStatus = VotingStatus.WaitingForPlayers;
        }
    }
}
