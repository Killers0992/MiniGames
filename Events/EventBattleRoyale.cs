using Exiled.API.Enums;
using Exiled.API.Features;
using MEC;
using MiniGames.Api;
using System;
using System.Collections.Generic;
using UnityEngine;
using Exiled.Events.EventArgs;
using MiniGames.Extensions;
using System.Linq;

namespace MiniGames.Events
{
    public class EventBattleRoyale : IEvent
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

        public string Description { get; } = "Team up with friends and kill every other team.";

        public string EventName
        {
            get
            {
                return "BattleRoyale";
            }
        }

        public EventStatus eventstatus;
        public int time;
        public bool broadcasting;
        public string counter;
        public string BattleRoyaleZoneState;
        public List<TeamClass> teamclasses;
        public List<Room> rooms;
        public Dictionary<Player, PlayerInfo> playersinfos;
        public Vector3 surfacegatepos;
        public List<string> doorsToNotOpen = new List<string>
        {
            "LCZ_ARMORY",
            "914",
            "NUKE_ARMORY",
            "HCZ_ARMORY",
            "GATE_A",
            "GATE_B",
            "096",
            "ESCAPE_INNER",
            "NUKE_SURFACE",
            "HID",
            "INTERCOM"
        };
        public List<string> doorsToLock = new List<string>
        {
            "NUKE_SURFACE",
            "ESCAPE_INNER"
        };
        public class TeamClass
        {
            public int MaxPlayers { get; set; }
            public List<Player> PlayersList { get; set; }
            public int TeamID { get; set; }
            public Vector3 TeleportPosition { get; set; }
            public string OwnerUserID { get; set; }
            public List<PlayerState> PlayersStatus { get; set; }
        }

        public class PlayerInfo
        {
            public int CurrentTeamID { get; set; }
            public int SecondsAlive { get; set; }
            public string CurrentBroadcast { get; set; }
        }

        public class Room
        {
            public bool IsOccupied { get; set; }
            public string RoomName { get; set; }
            public ZoneType Zone { get; set; }
            public Transform Object { get; set; }
        }

        public class PlayerState
        {
            public string NickName { get; set; }
            public string UserID { get; set; }
        }

        public void Start()
        {
            eventstatus = Api.EventStatus.Preparing;
            broadcasting = true;
            this.rooms = new List<Room>();
            playersinfos = new Dictionary<Player, PlayerInfo>();
            GameCore.RoundStart.LobbyLock = true;
            RoundSummary.RoundLock = true;
            MinigameSettings.MTFRespawnDisabled = true;
            MinigameSettings.DisableEscape = true;
            MinigameSettings.FriendlyFire = true;
            MinigameSettings.TeslaDisabled = true;
            foreach(var door in MinigamesExtensions.GetDoorsWithNames(doorsToNotOpen, false))
            {
                door.IsOpen = true;
            }

            foreach (var door in MinigamesExtensions.GetDoorsWithNames(doorsToLock, true))
            {
                door.ChangeLock(DoorLockType.AdminCommand);
            }

            surfacegatepos = MinigamesExtensions.GetDoorPositionByName("SURFACE_GATE");

            MinigamesExtensions.LockAllLifts();
            MinigamesExtensions.DestroyAllSinkHoles();
            MinigamesExtensions.LockAllCheckpoints(true, true);
            this.time = 30;
            this.counter = "2:00";
            EventManager.eventCoroutines.Add(Timing.RunCoroutine(StartProcedure()));
            teamclasses = new List<TeamClass>();
            EventManager.eventCoroutines.Add(Timing.RunCoroutine(BroadCastInfo()));
            foreach (var p in Player.List)
            {
                playersinfos.Add(p, new PlayerInfo() { CurrentBroadcast = "", CurrentTeamID = 0, SecondsAlive = 0 });
                p.IsGodModeEnabled = true;
                this.CreateTeam(p);
            }

        }

        public IEnumerator<float> StartProcedure()
        {
            bool end = true;
            while (end)
            {
                yield return Timing.WaitForSeconds(1f);
                time--;
                if (time == 0)
                {
                    end = false;
                    counter = "0:00";
                    time = 0;
                }
                counter = TimeSpan.FromSeconds((double)time).ToString("mm':'ss");
            }
            eventstatus = Api.EventStatus.Started;
            EventManager.eventCoroutines.Add(Timing.RunCoroutine(CheckEnd()));
            MinigamesExtensions.LockAllLifts(false);
            rooms = new List<Room>();
            foreach (var room in Map.Rooms.OrderBy(p => p.Zone))
            {
                if (room.Zone == ZoneType.Surface || 
                    room.Zone == ZoneType.Unspecified)
                    continue;
                if (room.Type == RoomType.LczArmory || 
                    room.Type == RoomType.Lcz914 || 
                    room.Type == RoomType.Lcz173 || 
                    room.Type == RoomType.Lcz012 || 
                    room.Type == RoomType.Hcz939 ||
                    room.Type == RoomType.HczArmory ||
                    room.Type == RoomType.EzGateA || 
                    room.Type == RoomType.EzGateB ||
                    room.Type == RoomType.LczChkpA ||
                    room.Type == RoomType.LczChkpB ||
                    room.Type == RoomType.HczTesla ||
                    room.Type == RoomType.EzShelter ||
                    room.Type == RoomType.HczNuke ||
                    room.Type == RoomType.Pocket ||
                    room.Type == RoomType.EzCollapsedTunnel)
                    continue;
                rooms.Add(new Room()
                {

                    IsOccupied = false,
                    Object = room.Transform,
                    RoomName = room.Name,
                    Zone = room.Zone
                });
            }
            foreach (Room room in this.rooms)
            {
                try
                {
                    int rng = UnityEngine.Random.Range(0, 10);
                    switch (rng)
                    {
                        case 0:
                            MinigamesExtensions.SpawnItem(ItemType.ArmorHeavy, room.Object.position);
                            MinigamesExtensions.SpawnItem(ItemType.Medkit, room.Object.position);
                            MinigamesExtensions.SpawnItem(ItemType.Medkit, room.Object.position);
                            MinigamesExtensions.SpawnItem(ItemType.Adrenaline, room.Object.position);
                            MinigamesExtensions.SpawnItem(ItemType.Medkit, room.Object.position);
                            MinigamesExtensions.SpawnItem(ItemType.Medkit, room.Object.position);
                            MinigamesExtensions.SpawnItem(ItemType.Medkit, room.Object.position);
                            break;
                        case 1:
                            MinigamesExtensions.SpawnItem(ItemType.ArmorHeavy, room.Object.position);
                            MinigamesExtensions.SpawnItem(ItemType.KeycardScientist, room.Object.position);
                            MinigamesExtensions.SpawnItem(ItemType.GunCrossvec, room.Object.position);
                            MinigamesExtensions.SpawnItem(ItemType.GunLogicer, room.Object.position);
                            MinigamesExtensions.SpawnItem(ItemType.Medkit, room.Object.position);
                            MinigamesExtensions.SpawnItem(ItemType.Medkit, room.Object.position);
                            break;
                        case 2:
                            MinigamesExtensions.SpawnItem(ItemType.KeycardNTFCommander, room.Object.position);
                            MinigamesExtensions.SpawnItem(ItemType.GunE11SR, room.Object.position);
                            MinigamesExtensions.SpawnItem(ItemType.GunAK, room.Object.position);
                            MinigamesExtensions.SpawnItem(ItemType.Medkit, room.Object.position);
                            MinigamesExtensions.SpawnItem(ItemType.Medkit, room.Object.position);
                            break;
                        case 3:
                            MinigamesExtensions.SpawnItem(ItemType.ArmorHeavy, room.Object.position);
                            MinigamesExtensions.SpawnItem(ItemType.KeycardContainmentEngineer, room.Object.position);
                            MinigamesExtensions.SpawnItem(ItemType.GunRevolver, room.Object.position);
                            MinigamesExtensions.SpawnItem(ItemType.Medkit, room.Object.position);
                            MinigamesExtensions.SpawnItem(ItemType.GunShotgun, room.Object.position);
                            break;
                        case 4:
                            MinigamesExtensions.SpawnItem(ItemType.KeycardGuard, room.Object.position);
                            MinigamesExtensions.SpawnItem(ItemType.Medkit, room.Object.position);
                            MinigamesExtensions.SpawnItem(ItemType.Medkit, room.Object.position);
                            MinigamesExtensions.SpawnItem(ItemType.GunShotgun, room.Object.position);
                            break;
                        case 5:
                            MinigamesExtensions.SpawnItem(ItemType.ArmorLight, room.Object.position);
                            MinigamesExtensions.SpawnItem(ItemType.KeycardNTFCommander, room.Object.position);
                            MinigamesExtensions.SpawnItem(ItemType.Medkit, room.Object.position);
                            break;
                        case 6:
                            MinigamesExtensions.SpawnItem(ItemType.GunShotgun, room.Object.position);
                            MinigamesExtensions.SpawnItem(ItemType.Coin, room.Object.position);
                            MinigamesExtensions.SpawnItem(ItemType.SCP207, room.Object.position);
                            break;
                        case 7:
                            MinigamesExtensions.SpawnItem(ItemType.Adrenaline, room.Object.position);
                            MinigamesExtensions.SpawnItem(ItemType.Adrenaline, room.Object.position);
                            MinigamesExtensions.SpawnItem(ItemType.Medkit, room.Object.position);
                            MinigamesExtensions.SpawnItem(ItemType.Medkit, room.Object.position);
                            break;
                        case 8:
                            MinigamesExtensions.SpawnItem(ItemType.GunShotgun, room.Object.position);
                            MinigamesExtensions.SpawnItem(ItemType.ArmorHeavy, room.Object.position);
                            MinigamesExtensions.SpawnItem(ItemType.Medkit, room.Object.position);
                            break;
                        case 9:
                            MinigamesExtensions.SpawnItem(ItemType.GrenadeHE, room.Object.position);
                            MinigamesExtensions.SpawnItem(ItemType.Medkit, room.Object.position);
                            MinigamesExtensions.SpawnItem(ItemType.GrenadeHE, room.Object.position);
                            break;
                        case 10:
                            MinigamesExtensions.SpawnItem(ItemType.GunShotgun, room.Object.position);
                            MinigamesExtensions.SpawnItem(ItemType.Medkit, room.Object.position);
                            MinigamesExtensions.SpawnItem(ItemType.Medkit, room.Object.position);
                            MinigamesExtensions.SpawnItem(ItemType.GunFSP9, room.Object.position);
                            break;
                    }
                }
                catch(Exception ex)
                {
                    Log.Error(ex.ToString());
                }
                
            }
            foreach (TeamClass teamClass in teamclasses)
            {
                try
                {
                    retry:
                    if (PlayerMovementSync.FindSafePosition(this.GetRandomPos(), out Vector3 safePos, true, true))
                        teamClass.TeleportPosition = safePos;
                    else
                        goto retry;
                    foreach (Player p in teamClass.PlayersList)
                    {
                        try
                        {
                            p.SetRespawnLocation(teamClass.TeleportPosition);
                            p.SpawnWithLoadout(RoleType.ClassD, new List<ItemType>());
                            p.IsGodModeEnabled = false;
                            p.SetMaxAmmo();
                        }
                        catch(Exception ex2)
                        {
                            Log.Error(ex2.ToString());
                        }

                    }
                }
                catch (Exception ex)
                {
                    Log.Info(ex.ToString());
                }
            }
            EventManager.eventCoroutines.Add(Timing.RunCoroutine(ZonesDecontamination()));
        }

        public Vector3 GetRandomPos()
        {
            rooms.Shuffle();
            foreach (var room in rooms)
            {
                if (!room.IsOccupied)
                {
                    room.IsOccupied = true;
                    if (PlayerMovementSync.FindSafePosition(room.Object.position, out Vector3 pos, true, true))
                        return pos;
                }
            }
            return new Vector3(0f, 0f, 0f);
        }

        public IEnumerator<float> BroadCastInfo()
        {
            while (broadcasting)
            {
                yield return Timing.WaitForSeconds(1f);
                try
                {
                    if (eventstatus == Api.EventStatus.Preparing)
                    {
                        foreach (Player p in Player.List)
                        {
                            if (playersinfos.TryGetValue(p, out PlayerInfo pinfo))
                            {
                                string text = "";
                                string currentbroadcast = pinfo.CurrentBroadcast;
                                TeamClass teamClass = this.FindTeamWithID(pinfo.CurrentTeamID);
                                p.CustomInfo = $"<color=white>TeamID</color> <color=green>{pinfo.CurrentTeamID}</color>";
                                if (teamClass.PlayersList.Count == 1)
                                {
                                    text = string.Concat(new object[]
                                    {
                                        "TeamID<color=green> ",
                                        teamClass.TeamID,
                                        "</color>",
                                        Environment.NewLine,
                                        "<size=30>",
                                        Environment.NewLine,
                                        "[ <color=yellow>",
                                        teamClass.PlayersList[0].Id,
                                        "</color> ] ",
                                        teamClass.PlayersList[0].Nickname,
                                        Environment.NewLine,
                                        "</size><size=15>Remaining time <color=green>" + counter + "</color></size>",
                                        Environment.NewLine,
                                        $"<size=20>You can join other team by using command <color=yellow>.join <teamId></color></size>"
                                });
                                }
                                else if (teamClass.PlayersList.Count == 2)
                                {
                                    text = string.Concat(new object[]
                                    {
                                        "TeamID<color=green> ",
                                        teamClass.TeamID,
                                        "</color>",
                                        Environment.NewLine,
                                        "<size=30>",
                                        Environment.NewLine,
                                        "[ <color=yellow>",
                                        teamClass.PlayersList[0].Id,
                                        "</color> ] ",
                                        teamClass.PlayersList[0].Nickname,
                                        Environment.NewLine,
                                        "[ <color=yellow>",
                                        teamClass.PlayersList[1].Id,
                                        "</color> ] ",
                                        teamClass.PlayersList[1].Nickname,
                                        Environment.NewLine,
                                        "</size><size=15>Remaining time <color=green>" + counter + "</color></size>"
                                    });
                                }
                                else if (teamClass.PlayersList.Count == 3)
                                {
                                    text = string.Concat(new object[]
                                    {
                                        "TeamID<color=green> ",
                                        teamClass.TeamID,
                                        "</color>",
                                        Environment.NewLine,
                                        "<size=30>",
                                        Environment.NewLine,
                                        "[ <color=yellow>",
                                        teamClass.PlayersList[0].Id,
                                        "</color> ] ",
                                        teamClass.PlayersList[0].Nickname,
                                        Environment.NewLine,
                                        "[ <color=yellow>",
                                        teamClass.PlayersList[1].Id,
                                        "</color> ] ",
                                        teamClass.PlayersList[1].Nickname,
                                        Environment.NewLine,
                                        "[ <color=yellow>",
                                        teamClass.PlayersList[2].Id,
                                        "</color> ] ",
                                        teamClass.PlayersList[2].Nickname,
                                        Environment.NewLine,
                                        "</size><size=15>Remaining time <color=green>" + counter + "</color></size>"
                                    });
                                }
                                else if (teamClass.PlayersList.Count >= 4)
                                {
                                    text = string.Concat(new object[]
                                    {
                                        "TeamID<color=green> ",
                                        teamClass.TeamID,
                                        "</color>",
                                        Environment.NewLine,
                                        "<size=30>",
                                        Environment.NewLine,
                                        "[ <color=yellow>",
                                        teamClass.PlayersList[0].Id,
                                        "</color> ] ",
                                        teamClass.PlayersList[0].Nickname,
                                        Environment.NewLine,
                                        "[ <color=yellow>",
                                        teamClass.PlayersList[1].Id,
                                        "</color> ] ",
                                        teamClass.PlayersList[1].Nickname,
                                        Environment.NewLine,
                                        "[ <color=yellow>",
                                        teamClass.PlayersList[2].Id,
                                        "</color> ] ",
                                        teamClass.PlayersList[2].Nickname,
                                        Environment.NewLine,
                                        "[ <color=yellow>",
                                        teamClass.PlayersList[3].Id,
                                        "</color> ] ",
                                        teamClass.PlayersList[3].Nickname,
                                        Environment.NewLine,
                                        "</size><size=15>Remaining time <color=green>" + counter + "</color></size>"
                                    });
                                }
                                if (!string.IsNullOrEmpty(text))
                                {
                                    p.SetCHint(text);
                                }
                            }
                        }
                    }
                    else if (eventstatus == Api.EventStatus.Started)
                    {
                        foreach (Player p in Player.List)
                        {
                            if (playersinfos.TryGetValue(p, out PlayerInfo pinfo))
                            {
                                string text = "";
                                if (pinfo.CurrentTeamID == 0)
                                {
                                    p.CustomInfo = $"<color=white>TeamID</color> <color=green>{pinfo.CurrentTeamID}</color>";
                                    text = string.Concat(new object[]
                                    {
                                        "Teams alive: <color=green>",
                                        GetPlayersInTeam(),
                                        "</color> | Time alive: <color=green>",
                                        TimeSpan.FromSeconds((double)pinfo.SecondsAlive).ToString("mm':'ss"),
                                        "</color>"
                                    });
                                }
                                else
                                {
                                    pinfo.SecondsAlive++;
                                    p.CustomInfo = $"<color=white>TeamID</color> <color=green>{pinfo.CurrentTeamID}</color>";
                                    TeamClass teamClass = this.FindTeamWithID(pinfo.CurrentTeamID);
                                    string text8 = " <color=green>|</color> ";
                                    foreach (PlayerState playerState in teamClass.PlayersStatus)
                                    {
                                        text8 = text8 + playerState.NickName + " <color=green>|</color> ";
                                    }
                                    text = string.Concat(new object[]
                                    {
                                        "<size=25>",
                                        text8,
                                        "</size>",
                                        Environment.NewLine,
                                        "Teams left: <color=green>" + this.GetPlayersInTeam() + "</color>"
                                    });
                                }
                                if (!string.IsNullOrEmpty(text))
                                {
                                    p.SetCHint(text);
                                }
                            }
                        }
                    }
                }
                catch (Exception ex) 
                {
                    Log.Info(ex.ToString());
                }
            }
        }

        public TeamClass FindTeamWithID(int teamid)
        {
            TeamClass result = null;
            bool flag = false;
            foreach (TeamClass teamClass in teamclasses)
            {
                if (teamClass.TeamID == teamid)
                {
                    result = teamClass;
                    flag = true;
                }
            }
            if (!flag)
                return null;
            return result;
        }

        public string JoinToTeam(Player player, int teamid)
        {
            if (playersinfos.TryGetValue(player, out PlayerInfo pinfo))
            {
                if (eventstatus != Api.EventStatus.Preparing)
                    return "Command is disabled while in game.";

                TeamClass teamClass = this.FindTeamWithID(teamid);
                TeamClass teamClass2 = this.FindTeamWithID(pinfo.CurrentTeamID);

                if (teamClass == null)
                    return "This team cannot be found.";

                if (teamid == pinfo.CurrentTeamID)
                    return "This team cannot be found.";

                if (teamClass.PlayersList.Count == teamClass.MaxPlayers)
                    return "This team is full.";

                PlayerState playerState = this.GetPlayerState(player);
                if (playerState != null)
                {
                    teamClass2.PlayersStatus.Remove(playerState);
                }
                if (teamClass2.OwnerUserID == player.UserId)
                {
                    foreach (Player teamPlayer in teamClass2.PlayersList)
                    {
                        if (teamPlayer.UserId != teamClass2.OwnerUserID)
                        {
                            this.CreateTeam(teamPlayer);
                        }
                    }
                }
                else
                    teamClass2.PlayersList.Remove(player);
                Vector3 position = Vector3.zero;
                string text = null;
                foreach (Player gameObject2 in teamClass.PlayersList)
                {
                    if (gameObject2.UserId == teamClass.OwnerUserID)
                    {
                        position = gameObject2.Position;
                        text = gameObject2.Nickname;
                    }
                }
                player.SetRespawnLocation(position);
                player.SpawnWithLoadout(RoleType.Scientist, new List<ItemType>());
                player.IsGodModeEnabled = true;
                if (teamClass2.OwnerUserID == player.UserId)
                {
                    teamclasses.Remove(teamClass2);
                }
                pinfo.CurrentTeamID = teamid;
                teamClass.PlayersList.Add(player);
                teamClass.PlayersStatus.Add(new PlayerState
                {
                    NickName = player.Nickname,
                    UserID = player.UserId
                });
                return string.Concat(new object[]
                {
                    "You joined ",
                    text,
                    "'s team."
                });
            }
            return "Error while handling PlayerInfo.";
        }

        public string KickPlayerFromTeam(Player player, int id)
        {
            if (playersinfos.TryGetValue(player, out PlayerInfo pinfo))
            {
                TeamClass teamClass = this.FindTeamWithID(pinfo.CurrentTeamID);
                if (teamClass.OwnerUserID != player.UserId)
                    return "You must be a leader to use that command.";
                if (eventstatus != Api.EventStatus.Preparing)
                    return "Command is disabled while in game.";
                if (id == player.Id)
                {
                    foreach (Player player2 in teamClass.PlayersList)
                        this.CreateTeam(player2);
                    teamclasses.Remove(teamClass);
                    return "The team has been removed.";
                }
                foreach (Player gameObject in teamClass.PlayersList)
                {
                    if (gameObject.Id == id)
                    {
                        teamClass.PlayersStatus.Remove(this.GetPlayerState(gameObject));
                        teamClass.PlayersList.Remove(gameObject);
                        this.CreateTeam(gameObject);
                        return "Player " + gameObject.Nickname + " has been kicked.";
                    }
                }
                return "Player " + player.Nickname + " has been kicked.";
            }
            return "Error while handling PlayerInfo.";
        }

        public string LeaveFromTeam(Player player)
        {
            if (playersinfos.TryGetValue(player, out PlayerInfo pinfo))
            {
                if (eventstatus != Api.EventStatus.Preparing)
                    return "Command is disabled while in game.";
                TeamClass teamClass = this.FindTeamWithID(pinfo.CurrentTeamID);
                if (teamClass.OwnerUserID == player.UserId)
                {
                    teamclasses.Remove(teamClass);
                    foreach (Player player2 in teamClass.PlayersList)
                        this.CreateTeam(player2);
                    return "The team has been removed.";
                }
                foreach (Player gameObject in teamClass.PlayersList)
                {
                    if (gameObject.Id == player.Id)
                    {
                        teamClass.PlayersList.Remove(gameObject);
                        PlayerState playerState = this.GetPlayerState(player);
                        if (playerState != null)
                            teamClass.PlayersStatus.Remove(playerState);
                        this.CreateTeam(gameObject);
                    }
                }
                return "You left the team.";
            }
            return "Error while handling PlayerInfo.";
        }

        public IEnumerator<float> ZonesDecontamination()
        {
           // LightContainmentZoneDecontamination.DecontaminationController.Singleton._stopUpdating = false;
            //LightContainmentZoneDecontamination.DecontaminationController.Singleton.NetworkRoundStartTime = NetworkTime.time;
            while(!LightContainmentZoneDecontamination.DecontaminationController.Singleton._decontaminationBegun)
            {
                yield return Timing.WaitForSeconds(1f);
            }
            yield return Timing.WaitForSeconds(15f);
            Cassie.Message(". Warning . 5 minutes left to Heavy containment zone decontamination sequence", false, true);
            yield return Timing.WaitForSeconds(150f);
            Cassie.Message(". Danger . 1 minute left to Heavy containment zone decontamination sequence . Please escape immediately", false, true);
            yield return Timing.WaitForSeconds(45f);
            Cassie.Message(". Heavy containment zone decontamination sequence has begun", false, true);
            foreach (var room in Map.Rooms)
            {
                if (room.Zone != ZoneType.HeavyContainment)
                    continue;
                foreach(var door in room.Doors)
                {
                    if (!door.IsOpen)
                        continue;
                    door.IsOpen = true;
                    door.ChangeLock(DoorLockType.AdminCommand);
                }
            }
            this.BattleRoyaleZoneState = "HCZ";
            yield return Timing.WaitForSeconds(30f);
            Cassie.Message(". Warning . 5 Minutes Left to Facility destruction", false, true);
            yield return Timing.WaitForSeconds(150f);
            PlayerManager.localPlayer.GetComponent<AlphaWarheadController>().InstantPrepare();
            MinigameSettings.WarheadDisabled = true;
            PlayerManager.localPlayer.GetComponent<AlphaWarheadController>().StartDetonation();
            yield break;
        }

        public int GetPlayersInTeam()
        {
            if (teamclasses.Count == 0)
                return 0;
            int alive = 0;
            foreach (TeamClass tc in teamclasses)
            {
                foreach (Player hub in tc.PlayersList)
                {
                    if (hub == null)
                        continue;
                    if (hub.Role == RoleType.ClassD)
                    {
                        alive++;
                        break;
                    }
                }
            }
            return alive;
        }

        public int GetPlayersAlive()
        {
            int players = 0;
            foreach (TeamClass tc in teamclasses)
            {
                foreach (Player p in tc.PlayersList)
                {
                    if (p.Team != Team.RIP)
                    {
                        players++;
                    }
                }
            }
            return players;
        }

        public PlayerState GetPlayerState(Player player)
        {
            TeamClass teamClass = this.FindTeamWithID(playersinfos[player].CurrentTeamID);
            if (teamClass == null)
            {
                return null;
            }
            bool flag = false;
            PlayerState result = null;
            foreach (PlayerState playerState in teamClass.PlayersStatus)
            {
                if (playerState.UserID == player.UserId)
                {
                    result = playerState;
                    flag = true;
                }
            }
            if (!flag)
            {
                return null;
            }
            return result;
        }

        public void CreateTeam(Player player)
        {
            if (playersinfos.TryGetValue(player, out PlayerInfo pinfo))
            {
                int num = UnityEngine.Random.Range(0, 999);
                bool flag = true;
                using (List<TeamClass>.Enumerator enumerator = teamclasses.GetEnumerator())
                {
                    while (enumerator.MoveNext())
                    {
                        if (enumerator.Current.TeamID == num)
                        {
                            flag = false;
                        }
                    }
                }
                if (!flag)
                {
                    this.CreateTeam(player);
                    return;
                }
                player.IsGodModeEnabled = true;
                player.SetRespawnLocation(new Vector3(51.88422f, 988.7299f, -58.86825f));
                player.SpawnWithLoadout(RoleType.NtfCaptain, new List<ItemType>());
                pinfo.CurrentTeamID = num;
                teamclasses.Add(new TeamClass
                {
                    MaxPlayers = MinigameSettings.MaxPlayers,
                    TeamID = num,
                    TeleportPosition = Vector3.zero,
                    PlayersStatus = new List<PlayerState>
                    {
                        new PlayerState
                        {
                            NickName = player.Nickname,
                            UserID = player.UserId
                        }
                    },
                    OwnerUserID = player.UserId,
                    PlayersList = new List<Player>
                    {
                        player
                    }
                });
            }
        }

        public void OnDeath(DiedEventArgs ev)
        {
            if (playersinfos.TryGetValue(ev.Target, out PlayerInfo pinfo))
            {
                if (pinfo.CurrentTeamID == 0)
                    return;
                TeamClass teamClass = FindTeamWithID(pinfo.CurrentTeamID);
                teamClass.PlayersList.Remove(ev.Target);
                GetPlayerState(ev.Target).NickName = $"<color=red>{ev.Target.Nickname}</color>";
                if (teamClass.PlayersList.Count < 0 || teamClass.PlayersList.Count == 0)
                {
                    teamclasses.Remove(teamClass);
                }
                pinfo.CurrentTeamID = 0;
            }
        }

        public void OnJoin(VerifiedEventArgs ev)
        {
            EventManager.eventCoroutines.Add(Timing.RunCoroutine(AddPlayer(ev.Player)));
        }


        public IEnumerator<float> AddPlayer(Player player)
        {
            yield return Timing.WaitForSeconds(3f);
            playersinfos.Add(player, new PlayerInfo() { CurrentBroadcast = "", CurrentTeamID = 0, SecondsAlive = 0 });
            if (eventstatus == Api.EventStatus.Preparing)
            {
                CreateTeam(player);
            }
        }

        public void OnHurt(HurtingEventArgs ev)
        {
            if (ev.Target == ev.Attacker)
                return;
            if (playersinfos.TryGetValue(ev.Target, out PlayerInfo targetPInfo))
            {
                if (playersinfos.TryGetValue(ev.Attacker, out PlayerInfo attackerPInfo))
                {
                    if (targetPInfo.CurrentTeamID == attackerPInfo.CurrentTeamID)
                    {
                        ev.Amount = 0f;
                        ev.IsAllowed = false;
                    }
                }
            }
        }

        public void OnLeave(DestroyingEventArgs ev)
        {
            if (playersinfos.TryGetValue(ev.Player, out PlayerInfo pinfo))
            {
                if (pinfo.CurrentTeamID != 0)
                {
                    TeamClass teamClass = FindTeamWithID(pinfo.CurrentTeamID);
                    teamClass.PlayersList.Remove(ev.Player);
                    if (eventstatus == Api.EventStatus.Preparing)
                    {
                        if (teamClass.OwnerUserID == ev.Player.UserId)
                        {
                            foreach (var plr in teamClass.PlayersList)
                            {
                                CreateTeam(plr);
                            }
                        }
                        return;
                    }
                    GetPlayerState(ev.Player).NickName = "<color=red>" + ev.Player.Nickname + "</color>";
                    if (teamClass.PlayersList.Count < 0 || teamClass.PlayersList.Count == 0)
                    {
                        teamclasses.Remove(teamClass);
                    }
                    pinfo.CurrentTeamID = 0;
                }
                playersinfos.Remove(ev.Player);
            }

        }

        public IEnumerator<float> CheckEnd()
        {
            yield return Timing.WaitForSeconds(10f);
            bool end = false;
            while (!end)
            {
                yield return Timing.WaitForSeconds(1f);
                int players = GetPlayersInTeam();
                if (eventstatus == Api.EventStatus.Started)
                {
                    if (players == 1)
                    {
                        string plrs = "";
                        foreach(var plr in teamclasses[0].PlayersStatus)
                        {
                            plrs += $" {plr.NickName} ";
                        }
                        eventstatus = Api.EventStatus.Ended;
                        Map.ClearBroadcasts();
                        Map.Broadcast(299, "Team " + teamclasses[0].TeamID + " won.\n" + plrs);
                        Warhead.Detonate();
                        EventManager.eventCoroutines.Add(Timing.RunCoroutine(EventManager.Roundrestart()));
                        end = true;
                        broadcasting = false;
                    }
                    else if (players == 0)
                    {
                        EventManager.eventCoroutines.Add(Timing.RunCoroutine(EventManager.Roundrestart()));
                        end = true;
                    }
                }
            }
            yield break;
        }
    }
}
