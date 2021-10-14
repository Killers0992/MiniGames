using Exiled.API.Features;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace MiniGames.Extensions
{
    public static class PlayerExtensions
    {
        public static System.Random rng = new System.Random();

        public static void Shuffle<T>(this IList<T> list)
        {
            int n = list.Count;
            while (n > 1)
            {
                n--;
                int k = rng.Next(n + 1);
                T value = list[k];
                list[k] = list[n];
                list[n] = value;
            }
        }

        public static void SpawnWithLoadout(this Player plr, RoleType role, List<ItemType> items)
        {
            plr.SetLoadout(items);
            plr.Role = role;
        }

        public static void SetLoadout(this Player plr, List<ItemType> items)
        {
            if (!plr.SessionVariables.ContainsKey("SPAWN_LOADOUT"))
                plr.SessionVariables.Add("SPAWN_LOADOUT", items);
            plr.SessionVariables["SPAWN_LOADOUT"] = items;
        }

        public static void SetRespawnLocation(this Player plr, Vector3 pos)
        {
            if (!plr.SessionVariables.ContainsKey("RESPAWN_POSITION"))
                plr.SessionVariables.Add("RESPAWN_POSITION", pos);
            plr.SessionVariables["RESPAWN_POSITION"] = pos;
        }

        public static void SetCHint(this Player plr, string msg)
        {
            if (!plr.SessionVariables.ContainsKey("CHINT"))
                plr.SessionVariables.Add("CHINT", "");

            plr.SessionVariables["CHINT"] = msg;
        }

        public static string GetCHint(this Player plr)
        {
            if (plr.TryGetSessionVariable<string>("CHINT", out string chint))
                return chint;

            plr.SessionVariables.Add("CHINT", "");
            return "";
        }

        public static void AddPickupBlacklist(this Player plr, ItemType item)
        {
            if (!plr.SessionVariables.ContainsKey("PICKUP_BLACKLIST"))
                plr.SessionVariables.Add("PICKUP_BLACKLIST", new List<ItemType>());

            ((List<ItemType>)plr.SessionVariables["PICKUP_BLACKLIST"]).Add(item);
        }

        public static void RemovePickupBlacklist(this Player plr, ItemType item)
        {
            if (!plr.SessionVariables.ContainsKey("PICKUP_BLACKLIST"))
                plr.SessionVariables.Add("PICKUP_BLACKLIST", new List<ItemType>());

            if (((List<ItemType>)plr.SessionVariables["PICKUP_BLACKLIST"]).Contains(item))
                ((List<ItemType>)plr.SessionVariables["PICKUP_BLACKLIST"]).Remove(item);
        }

        public static void ClearPickupBlacklist(this Player plr)
        {
            if (!plr.SessionVariables.ContainsKey("PICKUP_BLACKLIST"))
                plr.SessionVariables.Add("PICKUP_BLACKLIST", new List<ItemType>());

            ((List<ItemType>)plr.SessionVariables["PICKUP_BLACKLIST"]).Clear();
        }

        public static bool PickupBlackListContains(this Player plr, ItemType item)
        {
            if (plr.TryGetSessionVariable<List<ItemType>>("PICKUP_BLACKLIST", out List<ItemType> items))
                if (items.Contains(item))
                    return true;

            return false;
        }

        public static void SetMaxAmmo(this Player plr)
        {
            /*
            plr.Ammo[ItemType.Ammo12gauge] = 9999;
            plr.Ammo[ItemType.Ammo44cal] = 9999;
            plr.Ammo[ItemType.Ammo556x45] = 9999;
            plr.Ammo[ItemType.Ammo762x39] = 9999;
            plr.Ammo[ItemType.Ammo9x19] = 9999;          */
        }
    }
}
