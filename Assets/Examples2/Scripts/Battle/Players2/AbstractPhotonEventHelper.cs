using Prg.Scripts.Common.Photon;
using UnityEngine.Assertions;

namespace Examples2.Scripts.Battle.Players2
{
    /// <summary>
    /// Base class to send and receive Photon events for given player.
    /// </summary>
    /// <remarks>
    /// First byte in message payload  is reserved for player ID.
    /// </remarks>
    internal abstract class AbstractPhotonEventHelper
    {
        private readonly PhotonEventDispatcher _photonEventDispatcher;
        private readonly byte _msgId;
        private readonly byte _playerId;

        protected AbstractPhotonEventHelper(PhotonEventDispatcher photonEventDispatcher, byte msgId, byte playerId)
        {
            _photonEventDispatcher = photonEventDispatcher;
            _msgId = msgId;
            _playerId = playerId;
            _photonEventDispatcher.RegisterEventListener(msgId, data =>
            {
                var payload = (byte[])data.CustomData;
                Assert.IsTrue(payload.Length >= 1, "payload.Length >= 1");
                if (payload[0] == _playerId)
                {
                    OnMsgReceived(payload);
                }
            });
        }

        protected void RaiseEvent(byte[] payload)
        {
            Assert.IsTrue(payload.Length >= 1, "payload.Length >= 1");
            Assert.IsTrue(payload[0] == _playerId, "payload[0] == _playerId");
            _photonEventDispatcher.RaiseEvent(_msgId, payload);
        }

        protected abstract void OnMsgReceived(byte[] payload);
    }
}