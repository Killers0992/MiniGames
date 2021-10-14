using System;
using System.Collections.Generic;
using System.Linq;
using Exiled.API.Features;
using Exiled.Events.EventArgs;
using MEC;
using MiniGames.Api;
using MiniGames.Extensions;
using UnityEngine;

namespace MiniGames.Events
{
    public class EventHideAndSeek : IEvent  
    {
        public int EventStatus
        {
            get
            {
                return (int)eventstatus;
            }
            set
            {
                eventstatus = (EventStatus)value;
            }
        }

        public string Description { get; } = "Hide in LCZ and dont make noise while 939 is trying to kill you.";

        public string EventName
        {
            get
            {
                return "HideAndSeek";
            }
        }

        public EventStatus eventstatus;

        public void Start()
        {
            eventstatus = Api.EventStatus.Preparing;
            MinigameSettings.MTFRespawnDisabled = true;
            foreach(var door in MinigamesExtensions.GetDoors())
            {
                door.Value.TargetState = true;
                if (door.Key == "CHECKPOINT_LCZ_A" || door.Key == "CHECKPOINT_LCZ_B")
                {
                    door.Value.ServerChangeLock(Interactables.Interobjects.DoorUtils.DoorLockReason.AdminCommand, true);
                }
            }
            Cassie.Message("SCP 9 3 9 will be ready in . 30 . seconds", false, false);
            Map.ClearBroadcasts();
            Map.Broadcast(15, "<color=red>|</color> AutoEvent <color=red>|</color>\nHide And Seek");
            MinigamesExtensions.ClearAllItems();
            MinigamesExtensions.LockAllLifts();
            MinigamesExtensions.DestroyAllSinkHoles();
            EventManager.eventCoroutines.Add(Timing.RunCoroutine(this.Spawn939()));
            int cnt = Player.List.Count<Player>();
            EventManager.eventCoroutines.Add(Timing.RunCoroutine(CheckEnd()));
            int scp939 = cnt < 3 ? 1 : cnt < 3 ? 2: cnt < 6 ? 3 : 2;
            foreach (var p in Player.List)
            {
                if (scp939 != 0)
                {
                    scp939--;
                    p.SetRespawnLocation(new Vector3(51.88422f, 988.7299f, -58.86825f));
                    p.Role = RoleType.Scp93953;
                }
                else
                {
                    p.SpawnWithLoadout(RoleType.ClassD, new List<ItemType>());
                }
            }
        }

        public IEnumerator<float> Spawn939()
        {
            yield return Timing.WaitForSeconds(30f);
            Cassie.Message(". TERMINATION HAS . BEGUN", false, false);
            eventstatus = Api.EventStatus.Started;
            Vector3 doorPos = MinigamesExtensions.GetDoorPositionByName("LCZ_CAFE");
            foreach (var player in Player.List)
            {
                if (player.Role == RoleType.Scp93953)
                    player.Position = doorPos;
            }
            EventManager.eventCoroutines.Add(Timing.RunCoroutine(MinutesToEnd()));
        }

        public IEnumerator<float> MinutesToEnd()
        {
            yield return Timing.WaitForSeconds(300f);
            Cassie.Message("30 seconds left", false, false);
            yield return Timing.WaitForSeconds(25f);
            Cassie.Message("5 . 4 . 3 . 2 . 1 .", false, false);
            yield return Timing.WaitForSeconds(3f);
            eventstatus = Api.EventStatus.Ended;
            int classd = MinigamesExtensions.GetPlayersCount(RoleType.ClassD);
            foreach (var p in Player.List)
                p.IsGodModeEnabled = true;
            if (classd != 0)
                Map.Broadcast(299, "<color=red>|</color> AutoEvent <color=red>|</color>\nHiders won");
            else
                Map.Broadcast(299, "<color=red>|</color> AutoEvent <color=red>|</color>\nSeekers won");
            EventManager.eventCoroutines.Add(Timing.RunCoroutine(EventManager.Roundrestart()));
        }

        public void OnDeath(DiedEventArgs ev)
        {
            int players = MinigamesExtensions.GetPlayersCount(RoleType.ClassD);
            int players2 = MinigamesExtensions.GetPlayersCount(RoleType.Scp93953);
            if (players == 0 && players2 != 0)
            {
                eventstatus = Api.EventStatus.Ended;
                Map.ClearBroadcasts();
                Map.Broadcast(299, "<color=red>|</color> AutoEvent <color=red>|</color>\nSeekers won");
                EventManager.eventCoroutines.Add(Timing.RunCoroutine(EventManager.Roundrestart()));
            }
            if (players != 0 && players2 == 0)
            {
                eventstatus = Api.EventStatus.Ended;
                Map.ClearBroadcasts();
                Map.Broadcast(299, "<color=red>|</color> AutoEvent <color=red>|</color>\nHiders won");
                EventManager.eventCoroutines.Add(Timing.RunCoroutine(EventManager.Roundrestart()));
            }
        }

        public void OnJoin(VerifiedEventArgs ev)
        {
        }

        public void OnLeave(DestroyingEventArgs ev)
        {
        }

        public void OnHurt(HurtingEventArgs ev)
        {

        }

        public IEnumerator<float> CheckEnd()
        {
            yield return Timing.WaitForSeconds(10f);
            bool end = false;
            while (!end)
            {
                yield return Timing.WaitForSeconds(1f);
                int players = MinigamesExtensions.GetPlayersCount(RoleType.ClassD);
                int players2 = MinigamesExtensions.GetPlayersCount(RoleType.Scp93953);
                if (players == 0 && players2 != 0)
                {
                    eventstatus = Api.EventStatus.Ended;
                    Map.ClearBroadcasts();
                    Map.Broadcast(299, "<color=red>|</color> AutoEvent <color=red>|</color>\nSeekers won");
                    EventManager.eventCoroutines.Add(Timing.RunCoroutine(EventManager.Roundrestart()));
                    end = true;
                }
                else if (players != 0 && players2 == 0)
                {
                    eventstatus = Api.EventStatus.Ended;
                    Map.ClearBroadcasts();
                    Map.Broadcast(299, "<color=red>|</color> AutoEvent <color=red>|</color>\nHiders won");
                    EventManager.eventCoroutines.Add(Timing.RunCoroutine(EventManager.Roundrestart()));
                    end = true;
                }
                else if (players == 1 && players2 != 0)
                {
                    Map.ClearBroadcasts();
                    Map.Broadcast(299, "<color=red>|</color> AutoEvent <color=red>|</color>\nLast hider won");
                    eventstatus = Api.EventStatus.Ended;
                    EventManager.eventCoroutines.Add(Timing.RunCoroutine(EventManager.Roundrestart()));
                    end = true;
                }
                else if (players == 0 && players2 == 0)
                {
                    eventstatus = Api.EventStatus.Ended;
                    EventManager.eventCoroutines.Add(Timing.RunCoroutine(EventManager.Roundrestart()));
                    end = true;
                }
            }
            yield break;
        }
       
    }
}
