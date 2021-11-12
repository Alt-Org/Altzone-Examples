using System.Collections.Generic;
using System.Linq;
using Examples2.Scripts.Battle.Photon;
using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;
using Prg.Scripts.Common.Photon;
using UnityEngine;
using UnityEngine.Assertions;

namespace Examples2.Scripts.Connect
{
    /// <summary>
    /// Manages <c>Photon</c> <c>Player</c>s and associated <c>PlayerConnection</c>s when players enter or exit this room.
    /// </summary>
    public class ConnectionManager : MonoBehaviourPunCallbacks
    {
        [Header("Settings"), SerializeField] private PlayerConnection _player1;
        [SerializeField] private PlayerConnection _player2;
        [SerializeField] private PlayerConnection _player3;
        [SerializeField] private PlayerConnection _player4;

        private readonly List<PlayerConnection> _players = new List<PlayerConnection>();

        private void Awake()
        {
            Debug.Log($"Awake {PhotonNetwork.NetworkClientState}");
            _players.AddRange(new[] { _player1, _player2, _player3, _player4 });
        }

        public override void OnEnable()
        {
            base.OnEnable();
            Debug.Log($"OnEnable {PhotonNetwork.NetworkClientState}");
            if (PhotonNetwork.InRoom)
            {
                RoomIsReadyToPlay();
                enabled = false;
            }
            var state = PhotonNetwork.NetworkClientState;
            if (state == ClientState.PeerCreated || state == ClientState.Disconnected)
            {
                Debug.Log($"connect {PhotonNetwork.NetworkClientState}");
                var playerName = PhotonBattle.GetLocalPlayerName();
                PhotonLobby.connect(playerName);
                return;
            }
            throw new UnityException($"OnEnable: invalid connection state {PhotonNetwork.NetworkClientState}");
        }

        private void RoomIsReadyToPlay()
        {
            var room = PhotonNetwork.CurrentRoom;
            Debug.Log($"RoomIsReadyToPlay {PhotonNetwork.NetworkClientState} master {PhotonNetwork.IsMasterClient} players {room.PlayerCount}");
            foreach (var playerConnection in _players)
            {
                playerConnection.SetPhotonPlayer(null);
            }
            foreach (var player in room.GetPlayersByActorNumber())
            {
                AddPlayerToRoom(player);
            }
        }

        private void AddPlayerToRoom(Player player)
        {
            var room = PhotonNetwork.CurrentRoom;
            var playerPos = room.GetFreePlayerPosition();
            Debug.Log($"AddPlayerToRoom master {PhotonNetwork.IsMasterClient} {player.GetDebugLabel()} free playerPos {playerPos}");
            if (playerPos < 1 || playerPos > 4)
            {
                return;
            }
            var freePlayer = _players.FirstOrDefault(x => x.PlayerPos == playerPos);
            if (freePlayer != null)
            {
                Assert.IsTrue(!freePlayer.HasPlayer);
                freePlayer.SetPhotonPlayer(player);
            }
        }

        private void UpdatePlayerInRoom(Player player)
        {
            Debug.Log($"UpdatePlayerInRoom master {PhotonNetwork.IsMasterClient} {player.GetDebugLabel()}");
            var existingPlayer = _players.FirstOrDefault(x => player.Equals(x.Player));
            if (existingPlayer != null)
            {
                existingPlayer.UpdatePhotonPlayer(player);
            }
        }

        private void RemovePlayerFromRoom(Player player)
        {
            Debug.Log($"RemovePlayerFromRoom master {PhotonNetwork.IsMasterClient} {player.GetDebugLabel()}");
            var existingPlayer = _players.FirstOrDefault(x => player.Equals(x.Player));
            if (existingPlayer != null)
            {
                existingPlayer.SetPhotonPlayer(null);
            }
        }

        public override void OnConnectedToMaster()
        {
            Debug.Log($"OnConnectedToMaster -> joinLobby {PhotonNetwork.NetworkClientState}");
            PhotonLobby.joinLobby();
        }

        public override void OnJoinedLobby()
        {
            Debug.Log($"OnJoinedLobby -> createRoom {PhotonNetwork.NetworkClientState}");
            Hashtable customRoomProperties = new Hashtable()
            {
                { PhotonKeyNames.PlayerPosition1, (byte)0 },
                { PhotonKeyNames.PlayerPosition2, (byte)0 },
                { PhotonKeyNames.PlayerPosition3, (byte)0 },
                { PhotonKeyNames.PlayerPosition4, (byte)0 }
            };
            PhotonLobby.joinOrCreateRoom("testing", customRoomProperties);
        }

        public override void OnJoinedRoom()
        {
            Debug.Log($"OnJoinedRoom {PhotonNetwork.NetworkClientState}");
            var room = PhotonNetwork.CurrentRoom;
            var player = PhotonNetwork.LocalPlayer;
            if (!room.GetUniquePlayerNameForRoom(player, PhotonNetwork.NickName, "", out var uniquePlayerName))
            {
                // Make player name unique within this room if it was not!
                PhotonNetwork.NickName = uniquePlayerName;
            }
            RoomIsReadyToPlay();
        }

        public override void OnLeftRoom()
        {
            Debug.Log($"OnLeftRoom {PhotonNetwork.NetworkClientState}");
        }

        public override void OnPlayerEnteredRoom(Player newPlayer)
        {
            Debug.Log($"OnPlayerEnteredRoom {newPlayer.GetDebugLabel()}");
            AddPlayerToRoom(newPlayer);
        }

        public override void OnPlayerLeftRoom(Player otherPlayer)
        {
            Debug.Log($"OnPlayerLeftRoom {otherPlayer.GetDebugLabel()}");
            RemovePlayerFromRoom(otherPlayer);
        }

        public override void OnPlayerPropertiesUpdate(Player targetPlayer, Hashtable changedProps)
        {
            UpdatePlayerInRoom(targetPlayer);
        }

        public override void OnMasterClientSwitched(Player newMasterClient)
        {
            UpdatePlayerInRoom(newMasterClient);
        }
    }
}