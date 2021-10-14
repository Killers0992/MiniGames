using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Exiled.API.Enums;
using Exiled.API.Features;
using Exiled.Events.EventArgs;
using GameCore;
using InventorySystem;
using InventorySystem.Items;
using InventorySystem.Items.Pickups;
using MEC;
using MiniGames;
using MiniGames.Api;
using MiniGames.Extensions;
using UnityEngine;

namespace MiniGames.Events
{
    public class EventFFA : IEvent
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

        public string Description { get; } = "Everyone wants to kill you.";

        public string EventName
        {
            get
            {
                return "FreeForAll";
            }
        }

        public EventStatus eventstatus;

        public void Start()
        {
            eventstatus = Api.EventStatus.Preparing;
            MinigameSettings.MTFRespawnDisabled = true;
            MinigameSettings.FriendlyFire = true;
            MinigamesExtensions.ClearAllItems();
            MinigamesExtensions.LockAllLifts();
            MinigamesExtensions.DestroyAllSinkHoles();
            MinigamesExtensions.LockAllCheckpoints(true, true);
            MinigamesExtensions.SpawnInRandomRooms(Player.List, ZoneType.LightContainment, RoleType.ClassD, new List<ItemType>() 
            {
                ItemType.GunE11SR,
                ItemType.Medkit,
                ItemType.Medkit
            }, true, new List<RoomType>()
            {
                RoomType.LczArmory,
                RoomType.Lcz914,
                RoomType.LczGlassBox,
                RoomType.Lcz173,
                RoomType.Lcz012,
                RoomType.LczChkpA,
                RoomType.LczChkpB,
                RoomType.LczCurve
            });
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
                foreach(var plr in Player.List)
                {
                    plr.SetCHint($"\nPlayers alive <color=green>{players}</color>");
                }
                if (players == 1)
                {
                    eventstatus = Api.EventStatus.Ended;
                    string nick = "Unknown";
                    foreach (var player in Player.List)
                        if (player.Role == RoleType.ClassD)
                            nick = player.Nickname;
                    Map.ClearBroadcasts();
                    Cassie.Message("You are the . Alpha . Classd", false, false);
                    Map.Broadcast(299, "<color=red>|</color> AutoEvent <color=red>|</color>\nPlayer " + nick + " won.");
                    EventManager.eventCoroutines.Add(Timing.RunCoroutine(EventManager.Roundrestart()));
                    end = true;
                }
                else if (players == 0)
                {
                    eventstatus = Api.EventStatus.Ended;
                    end = true;
                    EventManager.eventCoroutines.Add(Timing.RunCoroutine(EventManager.Roundrestart()));
                }
            }
            yield break;
        }
    }
}
