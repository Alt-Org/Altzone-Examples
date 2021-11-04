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
        public static readonly List<IPlayerActor> AllPlayerActors = new List<IPlayerActor>();
        public static int HomeTeamIndex;
        public static int LocalTeamIndex;

        [Header("Live Data")] public int _playerPos;
        public int _teamIndex;
        public bool _isLocal;
        public int _oppositeTeamIndex;
        public int _teamMatePos;
        public bool _isAwake;

        private void Awake()
        {
            var photonView = PhotonView.Get(this);
            var player = photonView.Owner;
            PhotonBattle.getPlayerProperties(player, out _playerPos, out _teamIndex);
            _isLocal = photonView.IsMine;
            if (player.IsMasterClient)
            {
                // The player who created this room is in "home team"!
                HomeTeamIndex = _teamIndex;
                Debug.Log($"homeTeamIndex={HomeTeamIndex} pos={_playerPos}");
            }
            if (_isLocal)
            {
                Debug.Log($"localTeamIndex={LocalTeamIndex} pos={_playerPos}");
                LocalTeamIndex = _teamIndex;
            }
            _oppositeTeamIndex = GETOppositeTeamIndex(_teamIndex);
            _teamMatePos = GETTeamMatePos(_playerPos);
            Debug.Log($"Awake {player.NickName} pos={_playerPos} team={_teamIndex}");

            _isAwake = true; // Signal that we have configured ourself
        }

        private static int GETOppositeTeamIndex(int teamIndex)
        {
            return teamIndex == 0 ? 1 : 0;
        }

        private static int GETTeamMatePos(int playerPos)
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