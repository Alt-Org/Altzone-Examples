using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;
using UnityEngine.Assertions;

namespace Examples2.Scripts.Connect
{
    public class PlayerHandshake : MonoBehaviourPunCallbacks, IPunOwnershipCallbacks
    {
        [SerializeField] private PhotonView _photonView;
        [SerializeField] private int _playerId;
        [SerializeField] private int _instanceId;
        [SerializeField] private int _nameHash;

        private bool _isCustomPropertySet;

        public int InstanceId => _instanceId;
        
        public override void OnEnable()
        {
            base.OnEnable();
            PhotonNetwork.AddCallbackTarget(this);
            _photonView = PhotonView.Get(this);
            var localPlayer = PhotonNetwork.LocalPlayer;
            _instanceId = localPlayer.ActorNumber;
            Debug.Log(
                $"OnEnable {_playerId}:{_instanceId} {PhotonNetwork.NetworkClientState} mine {_photonView.IsMine} room {_photonView.IsRoomView}");
        }

        public override void OnDisable()
        {
            base.OnDisable();
            PhotonNetwork.RemoveCallbackTarget(this);
            Debug.Log(
                $"OnDisable {_playerId}:{_instanceId} {PhotonNetwork.NetworkClientState} mine {_photonView.IsMine} room {_photonView.IsRoomView}");
        }

        public void SetPlayerId(int playerId, ConnectInfo connectInfo)
        {
            _playerId = playerId;
        }

        [PunRPC]
        private void SendMessageRpc(int playerId, int instanceId, int nameHash)
        {
            Assert.IsTrue(playerId == _playerId, "playerId != _playerId");
            _nameHash = nameHash;
            Debug.Log($"RECV {_playerId}:{_instanceId}:{_nameHash:X} {_photonView.Controller.GetDebugLabel()}");
        }

        public override void OnPlayerPropertiesUpdate(Player targetPlayer, Hashtable changedProps)
        {
            if (!targetPlayer.Equals(_photonView.Controller))
            {
                return;
            }
            if (!targetPlayer.HasCustomProperty("i"))
            {
                return;
            }
            // Can send RPC
            var localPlayer = PhotonNetwork.LocalPlayer;
            _nameHash = localPlayer.NickName.GetHashCode();
            Debug.Log($"SEND {_playerId}:{_instanceId}:{_nameHash:X} {_photonView.Controller.GetDebugLabel()}");
            _photonView.RPC(nameof(SendMessageRpc), RpcTarget.All, _playerId, _instanceId, _nameHash);
        }

        void IPunOwnershipCallbacks.OnOwnershipRequest(PhotonView targetView, Player requestingPlayer)
        {
            // NOP
        }

        void IPunOwnershipCallbacks.OnOwnershipTransfered(PhotonView targetView, Player previousOwner)
        {
            if (_isCustomPropertySet)
            {
                return;
            }
            if (targetView.ViewID != _photonView.ViewID)
            {
                return;
            }
            var controller = _photonView.Controller;
            Debug.Log($"OnOwnershipTransfered {_playerId}:{_instanceId} {controller.GetDebugLabel()}");
            if (_photonView.IsMine && !controller.HasCustomProperty("i"))
            {
                _isCustomPropertySet = true;
                controller.SetCustomProperty("i", (byte)_instanceId);
            }
        }

        void IPunOwnershipCallbacks.OnOwnershipTransferFailed(PhotonView targetView, Player senderOfFailedRequest)
        {
            // NOP
        }
    }
}