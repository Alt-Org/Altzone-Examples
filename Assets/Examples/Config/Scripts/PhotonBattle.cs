using Examples.Model.Scripts.Model;
using ExitGames.Client.Photon;
using Photon.Realtime;
using System.Diagnostics;
using UnityEngine;

namespace Examples.Config.Scripts
{
    public static class PhotonBattle
    {
        public const string PlayerPositionKey = "pp";
        public const string PlayerMainSkillKey = "mk";

        public const int PlayerPositionGuest = 0;
        public const int PlayerPosition1 = 1;
        public const int PlayerPosition2 = 2;
        public const int PlayerPosition3 = 3;
        public const int PlayerPosition4 = 4;
        public const int PlayerPositionSpectator = 11;

        public const string TeamBlueKey = "tb";
        public const string TeamRedKey = "tr";

        public const int TeamBlueValue = 1;
        public const int TeamRedValue = 2;

        public const int StartPlayingEvent = 123;

        public static bool IsRealPlayer(Player player)
        {
            var playerPos = player.GetCustomProperty(PlayerPositionKey, -1);
            return playerPos >= PlayerPosition1 && playerPos <= PlayerPosition4;
        }

        public static void GetPlayerProperties(Player player, out int playerPos, out int teamIndex)
        {
            playerPos = player.GetCustomProperty(PlayerPositionKey, -1);
            if (playerPos == PlayerPosition1 || playerPos == PlayerPosition3)
            {
                teamIndex = TeamBlueValue;
            }
            else if (playerPos == PlayerPosition4 || playerPos == PlayerPosition2)
            {
                teamIndex = TeamRedValue;
            }
            else
            {
                teamIndex = -1;
            }
        }

        public static int GetOppositeTeamIndex(int teamIndex)
        {
            return teamIndex == TeamBlueValue ? TeamRedValue : TeamBlueValue;
        }

        public static int GetTeamMatePos(int playerPos)
        {
            switch (playerPos)
            {
                case PlayerPosition1:
                    return PlayerPosition3;
                case PlayerPosition2:
                    return PlayerPosition4;
                case PlayerPosition3:
                    return PlayerPosition1;
                case PlayerPosition4:
                    return PlayerPosition2;
                default:
                    throw new UnityException($"invalid player pos: {playerPos}");
            }
        }

        public static CharacterModel getPlayerCharacterModel(Player player)
        {
            var skillId = player.GetCustomProperty(PlayerMainSkillKey, -1);
            return Models.FindById<CharacterModel>(skillId);
        }

        [Conditional("UNITY_EDITOR")]
        public static void setDebugPlayerProps(Player player, int playerPos)
        {
            player.SetCustomProperties(new Hashtable
            {
                { PlayerPositionKey, playerPos },
                { PlayerMainSkillKey, (int)Defence.Deflection }
            });
            Debug.LogWarning($"setDebugPlayerProps {player.GetDebugLabel()}");
        }
    }
}