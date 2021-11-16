using System;
using System.Diagnostics;
using System.Linq;
using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;
using UnityEngine.Assertions;

namespace Examples2.Scripts.Battle.Photon
{
    /// <summary>
    /// Convenience class to manage some Photon related player stuff in one place.<br />
    /// <c>PlayerPosition</c> is specified in detail in GDD but it can have other values for other gameplay purposes, like spectator.<br />
    /// <c>TeamIndex is used for convenience to distinguish players in red or blue teams.</c><br />
    /// </summary>
    internal static class PhotonBattle
    {
        private const string PlayerNameKey = "PlayerData.PlayerName";
        private const string PlayerPositionKey = "pp";
        private const string PlayerMainSkillKey = "mk";
        private const string TeamBlueKey = "tb";
        private const string TeamRedKey = "tr";

        public const int PlayerPositionGuest = 0;
        public const int PlayerPosition1 = 1;
        public const int PlayerPosition2 = 2;
        public const int PlayerPosition3 = 3;
        public const int PlayerPosition4 = 4;
        public const int PlayerPositionSpectator = 11;

        public const int NoTeamValue = 0;
        public const int TeamBlueValue = 1;
        public const int TeamRedValue = 2;

        public static int CountRealPlayers()
        {
            return PhotonNetwork.CurrentRoom.Players.Values.Where(IsRealPlayer).Count();
        }

        public static string GetLocalPlayerName()
        {
            if (PhotonNetwork.InRoom)
            {
                return PhotonNetwork.NickName;
            }
            // TODO: this part need to be refactored to use the store system in the game when it is implemented.
            var playerName = PlayerPrefs.GetString(PlayerNameKey, string.Empty);
            if (string.IsNullOrWhiteSpace(playerName))
            {
                playerName = $"Player{1000 * (1 + DateTime.Now.Second % 10) + DateTime.Now.Millisecond:00}";
                PlayerPrefs.SetString(PlayerNameKey, playerName);
            }
            return playerName;
        }

        public static bool IsRealPlayer(Player player)
        {
            var playerPos = player.GetCustomProperty(PlayerPositionKey, PlayerPositionGuest);
            return playerPos >= PlayerPosition1 && playerPos <= PlayerPosition4;
        }

        public static int GetPlayerPos(Player player)
        {
            return player.GetCustomProperty(PlayerPositionKey, PlayerPositionGuest);
        }

        public static string GetTeamName(int teamIndex)
        {
            Assert.IsTrue(teamIndex >= TeamBlueValue && teamIndex <= TeamRedValue, $"Invalid team index: {teamIndex}");
            var room = PhotonNetwork.CurrentRoom;
            var key = teamIndex == TeamBlueValue ? TeamBlueKey : TeamRedKey;
            var teamName = room.GetCustomProperty(key, string.Empty);
            return !string.IsNullOrEmpty(teamName) ? teamName : $"?{teamIndex}?";
        }

        public static int GetTeamIndex(int playerPos)
        {
            switch (playerPos)
            {
                case PlayerPosition1:
                case PlayerPosition3:
                    return TeamBlueValue;
                case PlayerPosition2:
                case PlayerPosition4:
                    return TeamRedValue;
                default:
                    return NoTeamValue;
            }
        }

        [Conditional("UNITY_EDITOR")]
        public static void SetDebugPlayerPos(Player player, int playerPos)
        {
            player.SetCustomProperties(new Hashtable
            {
                { PlayerPositionKey, playerPos },
                { PlayerMainSkillKey, 1 }
            });
            Debug.Log($"setDebugPlayerProps {player.GetDebugLabel()}");
        }
    }
}