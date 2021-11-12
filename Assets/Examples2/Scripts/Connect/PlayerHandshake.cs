using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;

namespace Examples2.Scripts.Connect
{
    public class PlayerHandshake : MonoBehaviourPunCallbacks, IPunOwnershipCallbacks
    {
        [SerializeField] private PhotonView _photonView;

        public override void OnEnable()
        {
            base.OnEnable();
            PhotonNetwork.AddCallbackTarget(this);
            _photonView = PhotonView.Get(this);
            Debug.Log(
                $"OnEnable {PhotonNetwork.NetworkClientState} {_photonView}");
        }

        public override void OnDisable()
        {
            base.OnDisable();
            PhotonNetwork.RemoveCallbackTarget(this);
            Debug.Log(
                $"OnDisable {PhotonNetwork.NetworkClientState} {_photonView}");
        }

        [PunRPC]
        private void SendMessageRpc(int playerId, int instanceId, int nameHash)
        {
            Debug.Log($"RECV: {_photonView.Controller.GetDebugLabel()} {_photonView}");
        }

        public override void OnPlayerPropertiesUpdate(Player targetPlayer, Hashtable changedProps)
        {
            var localPlayer = PhotonNetwork.LocalPlayer;
            Debug.Log($"SEND: {_photonView.Controller.GetDebugLabel()} {_photonView}");
            _photonView.RPC(nameof(SendMessageRpc), RpcTarget.All, -1, -1, -1);
        }

        void IPunOwnershipCallbacks.OnOwnershipRequest(PhotonView targetView, Player requestingPlayer)
        {
            // NOP
        }

        void IPunOwnershipCallbacks.OnOwnershipTransfered(PhotonView targetView, Player previousOwner)
        {
            if (targetView.ViewID != _photonView.ViewID)
            {
                return;
            }
            var controller = _photonView.Controller;
            Debug.Log($"OnOwnershipTransfered {controller.GetDebugLabel()} {_photonView}");
        }

        void IPunOwnershipCallbacks.OnOwnershipTransferFailed(PhotonView targetView, Player senderOfFailedRequest)
        {
            // NOP
        }
    }
}