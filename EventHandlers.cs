using Exiled.API.Features;
using Exiled.Events.EventArgs;
using Exiled.Permissions.Extensions;
using MEC;
using MiniGames.Api;
using MiniGames.Events;
using MiniGames.Extensions;
using MiniGames.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace MiniGames
{
    public class EventHandlers
    {
        private MainClass plugin;
        
        public EventHandlers(MainClass plugin)
        {
            this.plugin = plugin;
        }

        public void OnWaitingForPlayers()
        {
            MinigameSettings.SetDefault();
            if (!ServerConsole.singleton.NameFormatter.commands.ContainsKey("current_event"))
                ServerConsole.singleton.NameFormatter.Commands.Add("current_event", delegate (List<string> args)
                {
                    return EventManager.activeEvent != null ? EventManager.activeEvent.EventName : "Waiting to start.";
                });
            if (plugin.Config.VotingEnabled)
            {
                GameCore.RoundStart.LobbyLock = true;
                plugin.emanager.StartVoting();
            }
        }

        public void OnRoundRestart()
        {
            EventManager.voteevents = new List<VoteEvent>();
            EventManager.playersvoted = new List<string>();
            EventManager.activeEvent = null;
            foreach (CoroutineHandle handle in EventManager.eventCoroutines)
                Timing.KillCoroutines(handle);
        }

        public void OnDestroy(DestroyingEventArgs ev)
        {
            if (EventManager.activeEvent != null)
                EventManager.activeEvent.OnLeave(ev);
        }

        public void OnVerified(VerifiedEventArgs ev)
        {
            if (MinigameSettings.VotingStatus != VotingStatus.Ended)
                Timing.RunCoroutine(Wait(ev.Player));

            if (EventManager.activeEvent != null)
                EventManager.activeEvent.OnJoin(ev);
        }

        public void OnEscape(EscapingEventArgs ev)
        {
            if (MinigameSettings.DisableEscape)
                ev.IsAllowed = false;
        }

        public void OnPlayerSpawn(SpawningEventArgs ev)
        {
            if (ev.Player.TryGetSessionVariable<Vector3>("RESPAWN_POSITION", out Vector3 pos))
            {
                ev.Position = pos;
                ev.Player.SessionVariables.Remove("RESPAWN_POSITION");
            }
        }

        public IEnumerator<float> Wait(Player p)
        {
            while (p.Role != RoleType.Spectator)
            {
                yield return Timing.WaitForOneFrame;
            }
            yield return Timing.WaitForSeconds(2f);
            p.Role = RoleType.Tutorial;
            if (p.CheckPermission("mg.global"))
            {
                p.Broadcast(5, "You have permission to use <color=green>MINIGAMES</color> command in RA for game settings.", Broadcast.BroadcastFlags.Normal, false);
            }
        }

        public void OnChangingRole(ChangingRoleEventArgs ev)
        {
            if (MinigameSettings.VotingStatus != VotingStatus.Ended)
            {
                if (ev.NewRole == RoleType.Spectator)
                    Timing.CallDelayed(2f, () => ev.Player.Role = RoleType.Tutorial);
                else
                    ev.NewRole = RoleType.Tutorial;
            }
            if (ev.Player.TryGetSessionVariable<List<ItemType>>("SPAWN_LOADOUT", out List<ItemType> items))
            {
                ev.Items.Clear();
                ev.Items.AddRange(items);
                ev.Player.SessionVariables.Remove("SPAWN_LOADOUT");
            }
        }

        public void OnWarheadStop(StoppingEventArgs ev)
        {
            if (MinigameSettings.WarheadDisabled)
                ev.IsAllowed = false;
        }

        public void OnTeamRespawn(RespawningTeamEventArgs ev)
        {
            if (MinigameSettings.MTFRespawnDisabled)
                ev.Players.Clear();
        }

        public void OnPlayerDeath(DiedEventArgs ev)
        {
            if (EventManager.activeEvent != null)
                EventManager.activeEvent.OnDeath(ev);
        }

        public void OnTriggerTesla(TriggeringTeslaEventArgs ev)
        {
            if (MinigameSettings.TeslaDisabled)
                ev.IsTriggerable = false;
        }

        public void OnActivate914(ActivatingEventArgs ev)
        {
            if (MinigameSettings.Scp914Disabled)
                ev.IsAllowed = false;
        }

        public void OnPlayerHurt(HurtingEventArgs ev)
        {
            if (EventManager.activeEvent != null)
                EventManager.activeEvent.OnHurt(ev);
        }

        public void OnPickupItem(PickingUpItemEventArgs ev)
        {
            if (ev.Player.PickupBlackListContains(ev.Pickup.Type))
                ev.IsAllowed = false;
        }
    }
}
