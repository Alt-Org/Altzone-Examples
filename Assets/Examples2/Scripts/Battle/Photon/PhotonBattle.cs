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
            var playerPos = player.GetCustomProperty(PlayerPositionKey, -1);
            return playerPos >= 0 && playerPos <= 3;
        }

        public static int GetPlayerPos(Player player)
        {
            return player.GetCustomProperty(PlayerPositionKey, -1);
        }

        public static string GetTeamName(int teamIndex)
        {
            Assert.IsTrue(teamIndex >= 0 && teamIndex <= 1, $"Invalid team index: {teamIndex}");
            var room = PhotonNetwork.CurrentRoom;
            var teamName = room.GetCustomProperty(teamIndex == 0 ? TeamBlueKey : TeamRedKey, string.Empty);
            if (!string.IsNullOrEmpty(teamName))
            {
                return teamName;
            }
            return $"?{teamIndex}?";
        }

        public static int GetTeamIndex(int playerPos)
        {
            switch (playerPos)
            {
                case 0:
                case 2:
                    return 0;
                case 1:
                case 3:
                    return 1;
                default:
                    return -1;
            }
        }

        [Conditional("UNITY_EDITOR")]
        public static void SetDebugPlayerPos(Player player, int playerPos)
        {
            player.SetCustomProperties(new Hashtable
            {
                { PlayerPositionKey, playerPos }
                //{ playerMainSkillKey, (int)Defence.Deflection }
            });
            Debug.Log($"setDebugPlayerProps {player.GetDebugLabel()}");
        }
    }
}