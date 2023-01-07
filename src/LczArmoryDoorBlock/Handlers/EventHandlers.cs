using System;
using System.Collections.Generic;
using System.Linq;
using Interactables.Interobjects.DoorUtils;
using MapGeneration;
using MEC;
using Mirror;
using PluginAPI.Core;
using PluginAPI.Core.Attributes;
using PluginAPI.Enums;
using UnityEngine;
using Random = UnityEngine.Random;

namespace LczArmoryDoorBlock
{
    internal sealed class EventHandlers
    {
        private const string HintFormat =
            "<line-height=95%><smallcaps><alpha=#88><align=\"center\"><voffset=-38em>{0}</voffset></align></smallcaps>";

        private static readonly HashSet<Player> _players;

        private static DateTime _lockTime;
        private static DoorVariant _armory;

        static EventHandlers()
            => _players = new (NetworkServer.maxConnections);

        private static Func<DoorVariant, bool> FindLczArmoryByName => door => FindName(door) == "LCZ_ARMORY";

        [PluginEvent(ServerEventType.RoundStart)]
        public void OnRoundStart()
        {
            _lockTime = DateTime.Now;

            FindLczArmory();
            LockArmory();
            DelayCall();
        }

        [PluginEvent(ServerEventType.PlayerInteractDoor)]
        public void OnPlayerInteractDoor(Player player, DoorVariant door, bool canOpen)
        {
            if (door != _armory || _armory.ActiveLocks == 0)
            {
                return;
            }

            SendLockedHint(player);

            SavePlayer(player);
        }

        private static void SavePlayer(Player player)
        {
            if (_players.Contains(player))
            {
                return;
            }

            _players.Add(player);
        }

        private static void UnlockArmory()
        {
            DisableLock();

            SendUnlockedHints();
        }

        private static void SendUnlockedHints()
        {
            foreach (Player player in _players)
            {
                if (!player.IsAlive || !IsInLcz(player.Position))
                {
                    continue;
                }

                SendUnlockedHint(player);
            }

            _players.Clear();
        }

        private static string FindName(DoorVariant door)
        {
            DoorNametagExtension nametag = door.GetComponent<DoorNametagExtension>();

            return string.IsNullOrEmpty(nametag.GetName) ? door.name.Trim() : nametag.GetName.Trim();
        }

        private static string GetLockedText()
            => "<size=125%><color=#FF0000>Дверь заблокирована!</color></size>"
               + $"<size=95%><color=#FFFF00>\nОна откроется через {GetSecondsEnding(Mathf.RoundToInt((float)(DateTime.Now - _lockTime).TotalSeconds))}.</color></size>";

        private static string GetUnlockedText()
            => "<size=125%><color=#00FF00>Дверь оружейной комнаты ЛКЗ разблокирована!</color></size>";

        private static string GetSecondsEnding(int seconds)
        {
            if (seconds % 100 >= 11 && seconds % 100 <= 14)
            {
                return $"{seconds} секунд";
            }

            switch (seconds % 10)
            {
                case 1:
                    return $"{seconds} секунду";
                case 2:
                case 3:
                case 4:
                    return $"{seconds} секунды";
                default:
                    return $"{seconds} секунд";
            }
        }

        private static void DelayCall() => Timing.CallDelayed(Random.Range(160, 240), UnlockArmory);

        private static void FindLczArmory()
            => _armory = DoorVariant.AllDoors.First(FindLczArmoryByName);

        private static void LockArmory()
            => _armory.ServerChangeLock(DoorLockReason.None, true);

        private static void SendLockedHint(Player player)
            => player.ReceiveHint(string.Format(HintFormat, GetLockedText()), 6);

        private static void SendUnlockedHint(Player player)
            => player.ReceiveHint(string.Format(HintFormat, GetUnlockedText()), 6);

        private static void DisableLock() => _armory.ServerChangeLock(DoorLockReason.None, false);

        private static bool IsInLcz(Vector3 pos) => RoomIdUtils.RoomAtPositionRaycasts(pos, false)?.Zone != FacilityZone.LightContainment;
    }
}