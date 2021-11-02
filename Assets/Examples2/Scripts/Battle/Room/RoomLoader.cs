using Photon.Pun;
using Photon.Realtime;
using Prg.Scripts.Common.Photon;
using System.Collections;
using UnityEngine;
using Hashtable = ExitGames.Client.Photon.Hashtable;

namespace Examples2.Scripts.Battle.Room
{
    /// <summary>
    /// Game room loader to establish a well known state for the room before actual gameplay starts.
    /// </summary>
    public class RoomLoader : MonoBehaviourPunCallbacks
    {
        [Header("Settings"), SerializeField] private bool isOfflineMode;
        [SerializeField] private int debugPlayerPos;
        [SerializeField] private GameObject[] objectsToActivate;

        private void Awake()
        {
            Debug.Log($"Awake: {PhotonNetwork.NetworkClientState}");
            if (PhotonNetwork.InRoom)
            {
                continueToNextStage();
                enabled = false;
            }
        }

        public override void OnEnable()
        {
            base.OnEnable();
            var state = PhotonNetwork.NetworkClientState;
            if (state == ClientState.PeerCreated || state == ClientState.Disconnected)
            {
                Debug.Log($"connect: {PhotonNetwork.NetworkClientState}");
                var playerName = PhotonBattle.getLocalPlayerName();
                PhotonNetwork.OfflineMode = isOfflineMode;
                if (isOfflineMode)
                {
                    PhotonNetwork.NickName = playerName;
                    PhotonNetwork.JoinRandomRoom();
                }
                else
                {
                    PhotonLobby.connect(playerName);
                }
                return;
            }
            throw new UnityException($"OnEnable: invalid connection state: {PhotonNetwork.NetworkClientState}");
        }

        private void continueToNextStage()
        {
            if (PhotonNetwork.IsMasterClient)
            {
                // Mark room "closed"
                PhotonLobby.closeRoom(keepVisible: true);
            }
            // Enable game objects when this room stage is ready to play
            StartCoroutine(activateObjects(objectsToActivate));
        }

        private static IEnumerator activateObjects(GameObject[] objectsToActivate)
        {
            // Enable game objects one per frame in array sequence
            for (var i = 0; i < objectsToActivate.LongLength; i++)
            {
                yield return null;
                objectsToActivate[i].SetActive(true);
            }
        }

        public override void OnConnectedToMaster()
        {
            if (!isOfflineMode)
            {
                Debug.Log($"joinLobby: {PhotonNetwork.NetworkClientState}");
                PhotonLobby.joinLobby();
            }
        }

        public override void OnJoinedLobby()
        {
            Debug.Log($"createRoom: {PhotonNetwork.NetworkClientState}");
            PhotonLobby.createRoom("testing");
        }

        public override void OnJoinedRoom()
        {
            PhotonBattle.setDebugPlayerPos(PhotonNetwork.LocalPlayer, debugPlayerPos);
        }

        public override void OnPlayerPropertiesUpdate(Player targetPlayer, Hashtable changedProps)
        {
            var player = PhotonNetwork.LocalPlayer;
            Debug.Log($"Start game for: {player.GetDebugLabel()}");
            continueToNextStage();
        }
    }
}