﻿using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Altzone.Scripts.Battle;
using Photon.Pun;
using Photon.Realtime;
using Prg.Scripts.Common.Photon;
using TMPro;
using UnityEngine;
using UnityEngine.Assertions;
using Hashtable = ExitGames.Client.Photon.Hashtable;

namespace Examples2.Scripts.Connect
{
    /// <summary>
    /// Manages <c>Photon</c> <c>Player</c>s and associated <c>PlayerConnection</c>s when players enter or exit this room.
    /// </summary>
    /// <remarks>
    /// We use room and player properties for player position bookkeeping.
    /// </remarks>
    public class ConnectionManager : MonoBehaviourPunCallbacks
    {
        [Header("Settings"), SerializeField] private PlayerConnection _player1;
        [SerializeField] private PlayerConnection _player2;
        [SerializeField] private PlayerConnection _player3;
        [SerializeField] private PlayerConnection _player4;
        [SerializeField] private TMP_Text _waitingText;

        private readonly List<PlayerConnection> _players = new List<PlayerConnection>();

        private void Awake()
        {
            Debug.Log($"Awake {PhotonNetwork.NetworkClientState}");
            _players.AddRange(new[] { _player1, _player2, _player3, _player4 });
            _waitingText.text = string.Empty;
        }

        public override void OnEnable()
        {
            base.OnEnable();
            Debug.Log($"OnEnable {PhotonNetwork.NetworkClientState}");
            if (PhotonNetwork.InRoom)
            {
                StartCoroutine(WaitForPlayerSetup());
                return;
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

        private IEnumerator WaitForPlayerSetup()
        {
            for (;;)
            {
                yield return null;
                var readyPlayerCount = 0;
                foreach (var playerConnection in _players)
                {
                    if (playerConnection.enabled)
                    {
                        readyPlayerCount += 1;
                    }
                }
                Debug.Log($"readyPlayerCount {readyPlayerCount}");
                if (readyPlayerCount == _players.Count)
                {
                    break;
                }
            }
            RoomIsReadyToPlay();
            enabled = false;
        }

        private void RoomIsReadyToPlay()
        {
            var room = PhotonNetwork.CurrentRoom;
            Debug.Log($"RoomIsReadyToPlay {PhotonNetwork.NetworkClientState} master {PhotonNetwork.IsMasterClient} players {room.PlayerCount}");
            Assert.IsTrue(room.HasCustomProperty(PhotonKeyNames.PlayerPosition1), "room.HasCustomProperty(PhotonKeyNames.PlayerPosition1)");
            foreach (var playerConnection in _players)
            {
                playerConnection.HidePhotonPlayer();
            }
            foreach (var player in room.GetPlayersByActorNumber())
            {
                AddPlayerToRoom(player);
            }
        }

        private void AddPlayerToRoom(Player player)
        {
            var room = PhotonNetwork.CurrentRoom;
            var playerPos = PhotonNetwork.IsMasterClient
                ? room.GetFreePlayerPosition()
                : player.GetCustomProperty(PhotonBattle.PlayerPositionKey, PhotonBattle.PlayerPositionGuest);
            Debug.Log($"AddPlayerToRoom master {PhotonNetwork.IsMasterClient} {player.GetDebugLabel()} free playerPos {playerPos}");
            if (playerPos < 1 || playerPos > 4)
            {
                return;
            }
            var freePlayer = _players.FirstOrDefault(x => x.PlayerPos == playerPos);
            if (freePlayer != null)
            {
                Assert.IsTrue(freePlayer.ActorNumber < 1);
                if (PhotonNetwork.IsMasterClient)
                {
                    var key = PhotonKeyNames.GetPlayerPositionRoomKey(playerPos);
                    room.SafeSetCustomProperty(key, playerPos, 0);
                    player.SetCustomProperty(PhotonBattle.PlayerPositionKey, playerPos);
                }
                freePlayer.ShowPhotonPlayer(player);
            }
        }

        private void RemovePlayerFromRoom(Player player)
        {
            Debug.Log($"RemovePlayerFromRoom master {PhotonNetwork.IsMasterClient} {player.GetDebugLabel()}");
            var existingPlayer = _players.FirstOrDefault(x => x.ActorNumber == player.ActorNumber);
            if (existingPlayer == null)
            {
                Debug.Log("not found");
                return;
            }
            if (PhotonNetwork.IsMasterClient)
            {
                var room = PhotonNetwork.CurrentRoom;
                var key = PhotonKeyNames.GetPlayerPositionRoomKey(existingPlayer.PlayerPos);
                room.SetCustomProperty(key, 0);
            }
            existingPlayer.HidePhotonPlayer();
        }

        private void UpdateAll()
        {
            var waitingCount = 0;
            var playerCount = 0;
            foreach (var player in PhotonNetwork.CurrentRoom.Players.Values)
            {
                var playerPos = player.GetCustomProperty(PhotonBattle.PlayerPositionKey, PhotonBattle.PlayerPositionGuest);
                if (playerPos == 0)
                {
                    waitingCount += 1;
                    continue;
                }
                var existingPlayer = _players.FirstOrDefault(x => x.ActorNumber == player.ActorNumber);
                if (existingPlayer == null)
                {
                    continue;
                }
                playerCount += 1;
                existingPlayer.UpdatePhotonPlayer(player);
            }
            _waitingText.text = playerCount < 4
                ? $"Missing players {4 - playerCount}"
                : waitingCount > 0
                    ? $"Waiting players {waitingCount}"
                    : string.Empty;
        }

        private void UpdatePlayerInRoom(Player player)
        {
            Debug.Log($"UpdatePlayerInRoom master {PhotonNetwork.IsMasterClient} {player.GetDebugLabel()}");
            var existingPlayer = _players.FirstOrDefault(x => x.ActorNumber == player.ActorNumber);
            if (existingPlayer != null)
            {
                existingPlayer.UpdatePhotonPlayer(player);
                return;
            }
            if (player.HasCustomProperty(PhotonBattle.PlayerPositionKey))
            {
                AddPlayerToRoom(player);
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
                { PhotonKeyNames.PlayerPosition1, 0 },
                { PhotonKeyNames.PlayerPosition2, 0 },
                { PhotonKeyNames.PlayerPosition3, 0 },
                { PhotonKeyNames.PlayerPosition4, 0 }
            };
            PhotonLobby.joinOrCreateRoom("testing", customRoomProperties);
        }

        public override void OnJoinedRoom()
        {
            Debug.Log($"OnJoinedRoom {PhotonNetwork.NetworkClientState}");
            var room = PhotonNetwork.CurrentRoom;
            var player = PhotonNetwork.LocalPlayer;
            if (!room.GetUniquePlayerNameForRoom(player, PhotonNetwork.NickName, string.Empty, out var uniquePlayerName))
            {
                // Make player name unique within this room if it was not!
                PhotonNetwork.NickName = uniquePlayerName;
            }
            RoomIsReadyToPlay();
        }

        public override void OnPlayerEnteredRoom(Player newPlayer)
        {
            Debug.Log($"OnPlayerEnteredRoom {newPlayer.GetDebugLabel()}");
            AddPlayerToRoom(newPlayer);
            UpdateAll();
        }

        public override void OnPlayerLeftRoom(Player otherPlayer)
        {
            Debug.Log($"OnPlayerLeftRoom {otherPlayer.GetDebugLabel()}");
            RemovePlayerFromRoom(otherPlayer);
            UpdateAll();
        }

        public override void OnPlayerPropertiesUpdate(Player targetPlayer, Hashtable changedProps)
        {
            UpdatePlayerInRoom(targetPlayer);
            UpdateAll();
        }

        public override void OnRoomPropertiesUpdate(Hashtable propertiesThatChanged)
        {
            var room = PhotonNetwork.CurrentRoom;
            if (room.PlayerCount < 4)
            {
                return;
            }
            foreach (var waitingPlayer in room.Players.Values)
            {
                var playerPos = waitingPlayer.GetCustomProperty(PhotonBattle.PlayerPositionKey, PhotonBattle.PlayerPositionGuest);
                if (playerPos > 0)
                {
                    continue;
                }
                AddPlayerToRoom(waitingPlayer);
                return;
            }
        }

        public override void OnMasterClientSwitched(Player newMasterClient)
        {
            UpdatePlayerInRoom(newMasterClient);
            UpdateAll();
        }
    }
}