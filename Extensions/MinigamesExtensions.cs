using Exiled.API.Enums;
using Exiled.API.Features;
using Exiled.API.Features.Items;
using Interactables.Interobjects;
using Interactables.Interobjects.DoorUtils;
using InventorySystem.Items.Pickups;
using MapGeneration.Distributors;
using Mirror;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace MiniGames.Extensions
{
    public static class MinigamesExtensions
    {
        public static Vector3 GetDoorPositionByName(string doorName)
        {
            if (DoorNametagExtension.NamedDoors.TryGetValue(doorName, out DoorNametagExtension doorNametagExtension))
                if (PlayerMovementSync.FindSafePosition(doorNametagExtension.transform.position, out Vector3 pos, true, true))
                    return pos;
            return new Vector3(0f, 0f, 0f);
        }

        public static void DestroyAllSinkHoles()
        {
            foreach(var hazard in UnityEngine.Object.FindObjectsOfType<SinkholeEnvironmentalHazard>())
            {
                NetworkServer.Destroy(hazard.gameObject);
            }
        }

        public static Pickup SpawnItem(ItemType item, Vector3 position, bool weaponWithMaxAmmo = true)
        {
            Item itemPick = new Item(item);
            Pickup p = null;
            try
            {
                p = itemPick.Spawn(position, Quaternion.identity);
            }   
            catch(Exception ex)
            {
                Log.Error($"Error while giving item " + item + " " + ex.ToString());
            }
            return p;
        }

        public static void LockAllLifts(bool isLocked = true)
        {
            foreach (Lift elevator in Map.Lifts)
                elevator.Network_locked = isLocked;
        }

        public static void LockAllCheckpoints(bool isLocked = true, bool isOpen = false)
        {
            foreach(Door door in Map.Doors)
            {
                if (door.Base is CheckpointDoor checkpoint)
                {
                    checkpoint.TargetState = isOpen;
                    checkpoint.ServerChangeLock(DoorLockReason.DecontEvacuate, isLocked);
                }
            }
        }

        public static List<Door> GetDoorsWithNames(List<string> doorsNames, bool include = true)
        {
            return Map.Doors.Where(p => !string.IsNullOrEmpty(p.Nametag) ? include ? doorsNames.Contains(p.Nametag.ToUpper()) : !doorsNames.Contains(p.Nametag) : false).ToList();
        }

        public static void ClearAllItems()
        {
           /* foreach (Locker locker in Map.Lockers)
                locker.Loot = new LockerLoot[0];
            foreach (var item in UnityEngine.Object.FindObjectsOfType<ItemPickupBase>())
                item.DestroySelf();  */
        }

        public static void SpawnInRandomRooms(IEnumerable<Player> players, ZoneType zone, RoleType role, List<ItemType> items, bool spawnWithMaxAmmo, List<RoomType> roomBlackList)
        {
            List<Room> rooms = Map.Rooms.Where(p => !roomBlackList.Contains(p.Type) && p.Zone == zone).ToList();
            foreach(var player in players)
            {
                retr:
                var rngRoom = rooms.Random();
                if (PlayerMovementSync.FindSafePosition(rngRoom.Position, out Vector3 safePos, true, true))
                    player.SetRespawnLocation(safePos);
                else
                    goto retr;
                player.SpawnWithLoadout(role, items);
                if (spawnWithMaxAmmo)
                    player.SetMaxAmmo();
            }
        }

        public static Dictionary<string, DoorVariant> GetDoors()
        {
            Dictionary<string, DoorVariant> doors = new Dictionary<string, DoorVariant>();
            foreach (var door in UnityEngine.Object.FindObjectsOfType<DoorVariant>())
            {
                if (door.TryGetComponent<DoorNametagExtension>(out DoorNametagExtension ex))
                {
                    doors.Add(ex.GetName, door);
                }
            }
            return doors;
        }

        public static int GetPlayersCount(RoleType role)
        {
            return Player.List.Where(p => p.Role == role).Count<Player>();
        }
    }
}
