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
    public class KickCommand : ICommand
    {
        public string Command { get; } = "kick";

        public string[] Aliases { get; }

        public string Description { get; } = "Kick someone from team.";

        public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
        {
            if (EventManager.activeEvent is Events.EventBattleRoyale ev)
            {
                if (arguments.Count == 1)
                {
                    if (int.TryParse(arguments.At(0), out int id))
                    {
                         response = ev.KickPlayerFromTeam(Player.Get(sender as PlayerCommandSender), id);
                        return true;
                    }
                    else
                    {
                        response = "Failed while parsing ID from string " + arguments.At(0);
                        return false;
                    }
                }
                else
                {
                    response = "Syntax: kick <playerId>";
                    return false;
                }
            }


            response = "Event battleroyale not running.";
            return false;
        }
    }
}
