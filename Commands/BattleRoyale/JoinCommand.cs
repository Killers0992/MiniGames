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
    public class JoinCommand : ICommand
    {
        public string Command { get; } = "join";

        public string[] Aliases { get; }

        public string Description { get; } = "Join someone team.";

        public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
        {
            if (EventManager.activeEvent is Events.EventBattleRoyale ev)
            {
                if (arguments.Count == 1)
                {
                    if (int.TryParse(arguments.At(0), out int id))
                    {
                        response = ev.JoinToTeam(Player.Get(sender as PlayerCommandSender), id);
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
                    response = "Syntax: join <playerId>";
                    return false;
                }
            }


            response = "Event battleroyale not running.";
            return false;
        }
    }
}
