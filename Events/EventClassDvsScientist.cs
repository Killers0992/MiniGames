using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Exiled.API.Features;
using Exiled.Events.EventArgs;
using GameCore;
using Interactables.Interobjects;
using MEC;
using MiniGames;
using MiniGames.Api;
using MiniGames.Extensions;
using UnityEngine;

namespace MiniGames.Events
{
    public class EventClassDvsScientist : IEvent
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
        public string Description { get; } = "Kill your enemies.";

        public string EventName
        {
            get
            {
                return "ClassDvsScientist";
            }
        }

        public EventStatus eventstatus;

        public void Start()
        {
            eventstatus = Api.EventStatus.Preparing;
            MinigameSettings.MTFRespawnDisabled = true;
            foreach (var door in MinigamesExtensions.GetDoors())
            {
                door.Value.TargetState = true;
                if (door.Key == "CHECKPOINT_LCZ_A" || door.Key == "CHECKPOINT_LCZ_B")
                {
                    foreach(var dr in (door.Value as CheckpointDoor)._subDoors)
                    {
                        if (dr is BreakableDoor bd)
                        {
                            bd.Network_destroyed = true;
                        }
                    }
                    door.Value.ServerChangeLock(Interactables.Interobjects.DoorUtils.DoorLockReason.AdminCommand, true);
                }
            }
            MinigamesExtensions.LockAllLifts();
            MinigamesExtensions.ClearAllItems();
            MinigamesExtensions.DestroyAllSinkHoles();
            MinigameSettings.FriendlyFire = false;
            Vector3 checkpoint_a = MinigamesExtensions.GetDoorPositionByName("CHECKPOINT_LCZ_A");
            Vector3 checkpoint_b = MinigamesExtensions.GetDoorPositionByName("CHECKPOINT_LCZ_B");
            int rnd = 0;
            foreach (var player in Player.List)
            {
                if (rnd == 0)
                {
                    rnd = 1;
                    player.SetRespawnLocation(checkpoint_a);
                    player.SpawnWithLoadout(RoleType.ClassD, new List<ItemType>()
                    {
                        ItemType.GunCOM18,
                        ItemType.Medkit,
                        ItemType.Medkit,
                        ItemType.Adrenaline
                    });
                    player.SetMaxAmmo();
                }
                else
                {
                    rnd = 0;
                    player.SetRespawnLocation(checkpoint_b);
                    player.SpawnWithLoadout(RoleType.Scientist, new List<ItemType>()
                    {
                        ItemType.GunCOM18,
                        ItemType.Medkit,
                        ItemType.Medkit,
                        ItemType.Adrenaline
                    });
                    player.SetMaxAmmo();
                }
            }
            EventManager.eventCoroutines.Add(Timing.RunCoroutine(CheckEnd()));
        }


        public void OnDeath(DiedEventArgs ev)
        {
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
                int players2 = MinigamesExtensions.GetPlayersCount(RoleType.Scientist);
                if (players == 0 && players2 != 0)
                {
                    eventstatus = Api.EventStatus.Ended;
                    Map.ClearBroadcasts();
                    Map.Broadcast(299, "<color=red>|</color> AutoEvent <color=red>|</color>\nScientists won.");
                    EventManager.eventCoroutines.Add(Timing.RunCoroutine(EventManager.Roundrestart()));
                    end = true;
                }
                else if (players != 0 && players2 == 0)
                {
                    eventstatus = Api.EventStatus.Ended;
                    Map.ClearBroadcasts();
                    Map.Broadcast(299, "<color=red>|</color> AutoEvent <color=red>|</color>\nClassD won.");
                    EventManager.eventCoroutines.Add(Timing.RunCoroutine(EventManager.Roundrestart()));
                    end = true;
                }
                else if (players != 0 && players2 == 0)
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
