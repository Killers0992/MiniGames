using System;
using System.Collections.Generic;
using System.Linq;
using Exiled.API.Enums;
using Exiled.API.Features;
using Exiled.Events.EventArgs;
using GameCore;
using MEC;
using MiniGames;
using MiniGames.Api;
using MiniGames.Extensions;
using UnityEngine;

namespace MiniGames.Events
{
    public class EventMurder : IEvent
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

        public string Description { get; } = "Murder wants to kill everyone and Detective needs to protect innocents.";

        public string EventName
        {
            get
            {
                return "Murder";
            }
        }

        public EventStatus eventstatus;

        public void Start()
        {
            eventstatus = Api.EventStatus.Preparing;
            MinigameSettings.MTFRespawnDisabled = true;
            MinigameSettings.FriendlyFire = true;
            MinigamesExtensions.LockAllCheckpoints(true, true);
            MinigamesExtensions.ClearAllItems();
            MinigamesExtensions.LockAllLifts();
            MinigamesExtensions.DestroyAllSinkHoles();
            List<Room> rooms = new List<Room>();
            foreach (Room room in Map.Rooms)
                if (room.Zone == ZoneType.LightContainment)
                    if (room.Type != RoomType.Lcz914 && room.Type != RoomType.LczArmory)
                        rooms.Add(room);
            string randomMurder = Player.List.Random().UserId;
            string randomDetective = Player.List.Where(p => p.UserId != randomMurder).ToList().Random().UserId;
            foreach(var hub in Player.List)
            {
                int rndnum = UnityEngine.Random.Range(0, rooms.Count);
                Vector3 roomPos = rooms[rndnum].Transform.position;
                hub.SetRespawnLocation(new Vector3(roomPos.x, roomPos.y + 2f, roomPos.z));
                if (hub.UserId == randomDetective)
                    hub.SpawnWithLoadout(RoleType.Scientist, new List<ItemType>()
                    {
                        ItemType.GunCOM18
                    });

                if (hub.UserId == randomDetective)
                {
                    hub.SetCHint("<b>You are</b> <color=blue>Detective</color>");
                }
                else
                {
                    hub.AddPickupBlacklist(ItemType.GunCOM15);
                    if (hub.UserId == randomMurder)
                    {
                        murders.Add(hub.UserId);
                        hub.SpawnWithLoadout(RoleType.ClassD, new List<ItemType>() { ItemType.GunCOM15 });
                        hub.Health = 50f;
                        hub.AddPickupBlacklist(ItemType.GunCOM18);
                        hub.SetCHint("<b>You are</b> <color=red>Murder</color>");
                    }
                    else
                    {
                        hub.Role = RoleType.ClassD;
                        hub.SetCHint("<b>You are</b> <color=green>Innocent</color>");
                    }
                    hub.SetMaxAmmo();
                }
            }
            EventManager.eventCoroutines.Add(Timing.RunCoroutine(CheckEnd()));
            EventManager.eventCoroutines.Add(Timing.RunCoroutine(DmgMurder()));
        }

        public void OnDeath(DiedEventArgs ev)
        {
            if (ev.Target != ev.Killer)
            {
                if (murders.Contains(ev.Killer.UserId))
                {
                    ev.Killer.Health = Mathf.Clamp(ev.Killer.Health + 10f, 0f, 100f);
                }
                else if (!murders.Contains(ev.Target.UserId))
                {
                    ev.Killer.ReferenceHub.playerStats.HurtPlayer(new PlayerStats.HitInfo(49f, "", DamageTypes.Nuke, 0, false), ev.Killer.GameObject);
                }
            }
            if (murders.Contains(ev.Target.UserId))
                murders.Remove(ev.Target.UserId);
        }

        public void OnJoin(VerifiedEventArgs ev)
        {
        }

        public void OnLeave(DestroyingEventArgs ev)
        {
            if (murders.Contains(ev.Player.UserId))
                murders.Remove(ev.Player.UserId);
        }

        public void OnHurt(HurtingEventArgs ev)
        {

        }

        public List<string> murders = new List<string>();

        public IEnumerator<float> DmgMurder()
        {
            while (true)
            {
                yield return Timing.WaitForSeconds(120f);
                foreach (var hub in Player.List)
                {
                    if (hub.Role != RoleType.Spectator)
                    {
                        if (murders.Contains(hub.UserId))
                        {
                            hub.ReferenceHub.playerStats.HurtPlayer(new PlayerStats.HitInfo(15f, "Event", DamageTypes.Contain, 0, false), hub.GameObject);
                        }
                    }
                }
            }
        }

        public IEnumerator<float> CheckEnd()
        {
            yield return Timing.WaitForSeconds(10f);
            bool end = false;
            while (!end)
            {
                yield return Timing.WaitForSeconds(1f);
                bool anyMurderIsAlive = false;
                int players = 0;
                foreach(var hub in Player.List)
                {
                    if (hub.Role != RoleType.Spectator)
                    {
                        if (murders.Contains(hub.UserId))
                        {
                            anyMurderIsAlive = true;
                        }
                        else
                        {
                            players++;
                        }
                    }
                }
                if (players == 0 && anyMurderIsAlive)
                {
                    eventstatus = Api.EventStatus.Ended;
                    Map.ClearBroadcasts();
                    Map.Broadcast(299, "<color=red>|</color> AutoEvent <color=red>|</color>\nMurder won");
                    EventManager.eventCoroutines.Add(Timing.RunCoroutine(EventManager.Roundrestart()));
                    end = true;
                }
                else if (players != 0 && !anyMurderIsAlive)
                {
                    eventstatus = Api.EventStatus.Ended;
                    Map.ClearBroadcasts();
                    Map.Broadcast(299, "<color=red>|</color> AutoEvent <color=red>|</color>\nMurder killed");
                    EventManager.eventCoroutines.Add(Timing.RunCoroutine(EventManager.Roundrestart()));
                    end = true;
                }
                else if (players == 0 && !anyMurderIsAlive)
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
