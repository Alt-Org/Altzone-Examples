using Examples.Config.Scripts;
using Examples.Game.Scripts.Battle.interfaces;
using Photon.Pun;
using System.Collections.Generic;
using UnityEngine;

namespace Examples.Game.Scripts.Battle.Player
{
    /// <summary>
    /// Helper to collect essential player data before all players can be enabled for the game play.
    /// </summary>
    public class PlayerActivator : MonoBehaviour
    {
        public static readonly List<IPlayerActor> allPlayerActors = new List<IPlayerActor>();
        public static int homeTeamIndex;

        [Header("Live Data")] public int playerPos;
        public int teamIndex;
        public bool isLocal;
        public int oppositeTeamIndex;
        public int teamMatePos;
        public bool isAwake;

        private void Awake()
        {
            var _photonView = PhotonView.Get(this);
            var player = _photonView.Owner;
            PhotonBattle.getPlayerProperties(player, out playerPos, out teamIndex);
            isLocal = _photonView.IsMine;
            if (player.IsMasterClient)
            {
                // The player who created this room is in "home team"!
                homeTeamIndex = teamIndex;
                Debug.Log($"homeTeamIndex={homeTeamIndex} pos={playerPos}");
            }
            oppositeTeamIndex = getOppositeTeamIndex(teamIndex);
            teamMatePos = getTeamMatePos(playerPos);
            Debug.Log($"Awake {player.NickName} pos={playerPos} team={teamIndex}");

            isAwake = true; // Signal that we have configured ourself
        }

        private static int getOppositeTeamIndex(int teamIndex)
        {
            return teamIndex == 0 ? 1 : 0;
        }

        private static int getTeamMatePos(int playerPos)
        {
            switch (playerPos)
            {
                case 0:
                    return 2;
                case 1:
                    return 3;
                case 2:
                    return 0;
                case 3:
                    return 1;
                default:
                    throw new UnityException($"invalid player pos: {playerPos}");
            }
        }
    }
}