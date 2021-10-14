using Exiled.API.Extensions;
using Exiled.API.Features;
using Exiled.API.Features.Items;
using Interactables.Interobjects.DoorUtils;
using InventorySystem;
using InventorySystem.Items;
using InventorySystem.Items.Firearms.Attachments;
using InventorySystem.Items.Pickups;
using MEC;
using MiniGames.Api;
using MiniGames.Events;
using MiniGames.Extensions;
using MiniGames.Models;
using Mirror;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

namespace MiniGames
{
    public class EventManager
    {
        private MainClass plugin;
        public EventHandlers eventHandlers;
        public static EventManager singleton;

        public EventManager(MainClass plugin)
        {
            singleton = this;
            this.plugin = plugin;
            this.eventHandlers = new EventHandlers(plugin);
            Exiled.Events.Handlers.Server.WaitingForPlayers += eventHandlers.OnWaitingForPlayers;
            Exiled.Events.Handlers.Server.RestartingRound += eventHandlers.OnRoundRestart;
            Exiled.Events.Handlers.Warhead.Stopping += eventHandlers.OnWarheadStop;
            Exiled.Events.Handlers.Server.RespawningTeam += eventHandlers.OnTeamRespawn;
            Exiled.Events.Handlers.Player.Verified += eventHandlers.OnVerified;
            Exiled.Events.Handlers.Player.Destroying += eventHandlers.OnDestroy;
            Exiled.Events.Handlers.Player.Died += eventHandlers.OnPlayerDeath;
            Exiled.Events.Handlers.Player.Hurting += eventHandlers.OnPlayerHurt;
            Exiled.Events.Handlers.Player.PickingUpItem += eventHandlers.OnPickupItem;
            Exiled.Events.Handlers.Scp914.Activating += eventHandlers.OnActivate914;
            Exiled.Events.Handlers.Player.TriggeringTesla += eventHandlers.OnTriggerTesla;
            Exiled.Events.Handlers.Player.ChangingRole += eventHandlers.OnChangingRole;
            Exiled.Events.Handlers.Player.Spawning += eventHandlers.OnPlayerSpawn;
            Exiled.Events.Handlers.Player.Escaping += eventHandlers.OnEscape;
        }


        public static List<string> playersvoted = new List<string>();
        public static List<VoteEvent> voteevents = new List<VoteEvent>();
        public static List<CoroutineHandle> eventCoroutines = new List<CoroutineHandle>();
        public static IEvent activeEvent = null;

        public static IEnumerator<float> Roundrestart()
        {
            yield return Timing.WaitForSeconds(5f);
            PlayerManager.localPlayer.GetComponent<PlayerStats>().Roundrestart();
        }

        public void StartVoting()
        {
            List<IEvent> events = new List<IEvent>
            {
                new EventBattleRoyale(),
                new EventMurder(),
                new EventFFA(),
                new EventHideAndSeek(),
                new EventClassDvsScientist()
            };
            System.Random rand = new System.Random();
            int[] array2 = (from x in Enumerable.Repeat<int>(0, events.Count).Select((int x, int i) => new
            {
                i = i,
                rand = rand.Next()
            })
            orderby x.rand
            select x.i).ToArray<int>();
            playersvoted = new List<string>();
            voteevents = new List<VoteEvent>();
            voteevents.Add(new VoteEvent
            {
                EventName = events[array2[0]].EventName,
                Votes = 0,
                EventID = array2[0],
                iEvent = events[array2[0]]
            });
            voteevents.Add(new VoteEvent
            {
                EventName = events[array2[1]].EventName,
                Votes = 0,
                EventID = array2[1],
                iEvent = events[array2[1]]
            });
            voteevents.Add(new VoteEvent
            {
                EventName = events[array2[2]].EventName,
                Votes = 0,
                EventID = array2[2],
                iEvent = events[array2[2]]
            });
            MinigameSettings.VotingStatus = VotingStatus.WaitingForPlayers;
            eventCoroutines.Add(Timing.RunCoroutine(this.BroadCastInfo()));
            eventCoroutines.Add(Timing.RunCoroutine(this.Counting()));
            RoundSummary.RoundLock = true;
            CharacterClassManager.ForceRoundStart();
        }

        public IEnumerator<float> BroadCastInfo()
        {
            while(true)
            {
                yield return Timing.WaitForSeconds(0.8f);
                foreach(Player p in Player.List)
                {
                    if (MinigameSettings.VotingStatus == VotingStatus.WaitingForPlayers)
                    {
                        string text = string.Concat(new object[]
                        {
                            Environment.NewLine,
                            Environment.NewLine,
                            Environment.NewLine,
                            Environment.NewLine,
                            Environment.NewLine,
                            "Waiting for players...",
                            Environment.NewLine,
                            $"<color=green>1/2</color> players."
                        });
                        p.ShowHint(text, 1f);
                    }
                    else if (MinigameSettings.VotingStatus == VotingStatus.Counting)
                    {
                        string text = string.Concat(new object[]
                        {
                            Environment.NewLine,
                            Environment.NewLine,
                            Environment.NewLine,
                            Environment.NewLine,
                            Environment.NewLine,
                            "[<color=yellow>1</color>] ",
                            voteevents[0].EventName,
                            " - <color=green>",
                            voteevents[0].Votes,
                            "</color> | [<color=yellow>2</color>] ",
                            voteevents[1].EventName,
                            " - <color=green>",
                            voteevents[1].Votes,
                            "</color> | [<color=yellow>3</color>] ",
                            voteevents[2].EventName,
                            " - <color=green>",
                            voteevents[2].Votes + "</color>"
                        });
                        if (playersvoted.Contains(p.UserId))
                        {
                            text = $"{text}\n<color=red>[</color> {MinigameSettings.VotingTime} <color=red>]</color>";
                            p.ShowHint(text, 1f);

                        }
                        else
                        {
                            text = $"{text}\nType .vote <NR> to make a choice";
                            text = $"{text}\n<color=red>[</color> {MinigameSettings.VotingTime} <color=red>]</color>";
                            p.ShowHint(text, 1f);
                        }
                    }
                    else if (MinigameSettings.VotingStatus == VotingStatus.NextEventSelected)
                    {
                        string text = $"Next Event <color=red>{voteevents[0].EventName}</color>";
                        text = $"{text}\n<color=red>[</color> {MinigameSettings.VotingTime} <color=red>]</color>";
                        p.ShowHint(text, 1f);
                    }
                    else if (MinigameSettings.VotingStatus == VotingStatus.Ended)
                    {
                        string msg = string.Concat("\n\n\n\n\n\n\n\n\n\n\n",
                            $"Event: <color=red>{activeEvent.EventName}</color>, ",
                            "\n",
                            $"Desc: <color=yellow>{activeEvent.Description}</color>",
                            "\n" + p.GetCHint());
                        p.ShowHint(msg, 1f);
                    }
                }
            }
        }

        public IEnumerator<float> Counting()
        {
            while(MinigameSettings.VotingStatus != VotingStatus.Ended)
            {
                yield return Timing.WaitForSeconds(1f);
                if (MinigameSettings.VotingStatus == VotingStatus.Ended)
                    break;
                if (MinigameSettings.VotingTime != 0)
                {
                    if (MinigameSettings.VotingStatus != VotingStatus.NextEventSelected)
                    {
                        if (Player.List.Count<Player>() >= 2)
                        {
                            MinigameSettings.VotingTime--;
                            MinigameSettings.VotingStatus = VotingStatus.Counting;
                        }
                        else
                        {
                            MinigameSettings.VotingTime = 40;
                            MinigameSettings.VotingStatus = VotingStatus.WaitingForPlayers;
                        }
                    }
                    else
                    {
                        MinigameSettings.VotingTime--;
                    }
                }
                else
                {
                    if (MinigameSettings.VotingStatus != VotingStatus.NextEventSelected)
                    {
                        MinigameSettings.VotingStatus = VotingStatus.NextEventSelected;
                        voteevents = (from o in voteevents
                                      orderby o.Votes descending
                                      select o).ToList<VoteEvent>();
                        MinigameSettings.SelectedEventID = voteevents[0].EventID;
                        MinigameSettings.VotingTime = 20;
                    }
                    else
                    {
                        MinigameSettings.VotingStatus = VotingStatus.Ended;
                        activeEvent = voteevents[0].iEvent;
                        activeEvent.Start();
                        yield break;
                    }
                }
            }
        }
    }
}
