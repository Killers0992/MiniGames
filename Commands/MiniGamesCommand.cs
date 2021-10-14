using CommandSystem;
using Exiled.API.Features;
using Exiled.API.Features.Items;
using Exiled.Permissions.Extensions;
using MiniGames.Api;
using MiniGames.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace MiniGames.Commands
{
    [CommandHandler(typeof(RemoteAdminCommandHandler))]
    public class MiniGamesCommand : ICommand
    {
        public string Command { get; } = "minigames";

        public string[] Aliases { get; }

        public string Description { get; } = "Minigames admin command.";

        public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
        {
            if (!sender.CheckPermission("mg.global"))
            {
                response = "Missing permission";
                return false;
            }
            if (arguments.Count == 0)
            {
                response = "Invalid argument: minigames addvotes/setvotes/instantstart/refreshevents/rainbow/maxteam";
                return false;
            }
            switch (arguments.At(0).ToUpper())
            {
                case "INSTANTSTART":
                    MinigameSettings.VotingTime = -1;
                    MinigameSettings.VotingStatus = VotingStatus.Ended;
                    EventManager.voteevents = (from o in EventManager.voteevents
                                               orderby o.Votes descending
                                               select o).ToList<VoteEvent>();
                    MinigameSettings.SelectedEventID = EventManager.voteevents[0].EventID;
                    EventManager.activeEvent = EventManager.voteevents[0].iEvent;
                    EventManager.activeEvent.Start();
                    response = "Force started event.";
                    return true;
                case "MAXTEAM":
                    if (int.TryParse(arguments.At(1), out int id))
                    {
                        MinigameSettings.MaxPlayers = id;
                        response = "Max people in battleroyale team is set to " + id;
                        return true;
                    }
                    else
                    {
                        response = "Syntax: MINIGAMES maxteam <amount>";
                        return false;
                    }
                    break;
                case "REFRESHEVENTS":
                    EventManager.singleton.eventHandlers.OnRoundRestart();
                    EventManager.singleton.StartVoting();
                    response = "Refreshing events...";
                    return true;
                case "SETVOTES":
                case "ADDVOTES":
                    if (int.TryParse(arguments.At(1), out int eventId))
                    {
                        if (int.TryParse(arguments.At(2), out int amount))
                        {
                            if (EventManager.voteevents[eventId - 1] == null)
                            {
                                response = "Invalid EventID.";
                                return false;
                            }
                            switch (arguments.At(0).ToUpper())
                            {
                                case "SETVOTES":
                                    EventManager.voteevents[eventId - 1].Votes = amount;
                                    response = $"Set {amount} votes to event {eventId}";
                                    return true;
                                case "ADDVOTES":
                                    EventManager.voteevents[eventId - 1].Votes = EventManager.voteevents[eventId - 1].Votes + amount;
                                    response = $"Added {amount} votes to event {eventId}";
                                    return true;
                                default:
                                    response = "Error";
                                    return false;
                            }
                        }
                    }
                    response = $"Error while parsing: minigames {arguments.At(0).ToLower()} <eventId> <amount>";
                    return false;
                case "GREN":
                    var plr = Player.Get((CommandSender)sender);
                    for (int x = 0; x < int.Parse(arguments.At(1)); x++)
                    {
                        var item = new Item((ItemType)int.Parse(arguments.At(2)));
                        item.Spawn(plr.Position, Quaternion.identity);
                    }
                    response = "Done";
                    return true;
                default:
                    response = "Subcommand not found, minigames addvotes/setvotes/instantstart/refreshevents/rainbow/maxteam";
                    return false;
            }
        }
    }
}
