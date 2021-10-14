using CommandSystem;
using RemoteAdmin;
using System;

namespace MiniGames.Commands
{
	[CommandHandler(typeof(ClientCommandHandler))]
	public class WikiCommand : ICommand
	{
		public string Command { get; } = "vote";

		public string[] Aliases { get; }

		public string Description { get; } = "Vote for next game.";

		public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
		{
            if (MinigameSettings.VotingStatus == Api.VotingStatus.Ended)
            {
                response = "Voting ended.";
                return false;
            }
            var player = sender as PlayerCommandSender;
            if (arguments.Count <= 0)
            {
                response = ".vote 1/2/3 ";
                return false;
            }
            if (!(arguments.At(0) == "1") && !(arguments.At(0) == "2") && !(arguments.At(0) == "3"))
            {
                response = "Wrong syntax";
                return false;
            }
            if (!EventManager.playersvoted.Contains(player.SenderId))
            {
                string str = "ERROR";
                if (arguments.At(0) == "1")
                {
                    EventManager.voteevents[0].Votes++;
                    str = EventManager.voteevents[0].EventName;
                }
                else if (arguments.At(0) == "2")
                {
                    EventManager.voteevents[1].Votes++;
                    str = EventManager.voteevents[1].EventName;
                }
                else if (arguments.At(0) == "3")
                {
                    EventManager.voteevents[2].Votes++;
                    str = EventManager.voteevents[2].EventName;
                }
                EventManager.playersvoted.Add(player.SenderId);
                response = "You voted for event " + str;
                return true;
            }
            response = "You've already voted.";
            return true;
		}
	}
}
