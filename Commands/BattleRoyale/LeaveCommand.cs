using CommandSystem;
using Exiled.API.Features;
using RemoteAdmin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniGames.Commands.BattleRoyale
{
    [CommandHandler(typeof(ClientCommandHandler))]
    public class LeaveCommand : ICommand
    {
        public string Command { get; } = "leave";

        public string[] Aliases { get; }

        public string Description { get; } = "Leave team.";

        public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
        {
            if (EventManager.activeEvent is Events.EventBattleRoyale ev)
            {
                response = ev.LeaveFromTeam(Player.Get(sender as PlayerCommandSender));
                return true;
            }


            response = "Event battleroyale not running.";
            return false;
        }
    }
}
