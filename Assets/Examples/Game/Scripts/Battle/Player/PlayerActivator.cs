using Examples.Config.Scripts;
using Photon.Pun;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Examples.Game.Scripts.Battle.Player
{
    /// <summary>
    /// Synchronize local player(s) activation before "everything else" in the room.
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
            if (isLocal && player.IsMasterClient)
            {
                homeTeamIndex = teamIndex;
                Debug.Log($"homeTeamIndex={homeTeamIndex} pos={playerPos}");
            }
            oppositeTeamIndex = getOppositeTeamIndex(teamIndex);
            teamMatePos = getTeamMatePos(playerPos);
            Debug.Log($"Awake {player.NickName} pos={playerPos} team={teamIndex}");

            isAwake = true; // Signal that we have configured ourself
        }

        public IPlayerActor getTeamMate()
        {
            return allPlayerActors.FirstOrDefault(x => x.TeamIndex == teamIndex && x.PlayerPos != playerPos);
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