using System;
using Altzone.Scripts.Battle;
using Examples2.Scripts.Battle.interfaces;
using Photon.Pun;
using Prg.Scripts.Common.Photon;
using UnityEngine;

namespace Examples2.Scripts.Battle.Players2
{
    internal class PlayerShield2 : IPlayerShield
    {
        private const byte MsgSetShield = PhotonEventDispatcher.EventCodeBase + 7;

        private readonly ShieldConfig _config;
        private readonly ShieldStateHelper _stateHelper;

        public PlayerShield2(ShieldConfig config, PhotonView photonView)
        {
            _config = config;
            var playerId = (byte)photonView.OwnerActorNr;
            _stateHelper = new ShieldStateHelper(PhotonEventDispatcher.Get(), MsgSetShield, playerId, SetShieldState);
        }

        void IPlayerShield.SetupShield(int playerPos)
        {
            if (playerPos > PhotonBattle.PlayerPosition2)
            {
                var shield = _config.Shields[0];
                var renderer = shield.GetComponent<SpriteRenderer>();
                renderer.flipY = false;
            }
        }

        void IPlayerShield.SetShieldState(int playMode, int rotationIndex)
        {
            Debug.Log($"SetShieldState mode {playMode} rotation {rotationIndex}");
            _stateHelper.SetShieldState(playMode, rotationIndex);
        }

        void IPlayerShield.PlayHitEffects()
        {
            throw new NotImplementedException();
        }

        private void SetShieldState(int playMode, int rotationIndex)
        {
        }

        private class ShieldStateHelper : AbstractPhotonEventHelper
        {
            private readonly Action<int, int> _callback;

            private readonly byte[] _buffer = new byte[1 + 1 + 1];

            public ShieldStateHelper(PhotonEventDispatcher photonEventDispatcher, byte msgId, byte playerId, Action<int, int> onSetShieldState)
                : base(photonEventDispatcher, msgId, playerId)
            {
                _callback = onSetShieldState;
            }

            public void SetShieldState(int playMode, int rotationIndex)
            {
                _buffer[1] = (byte)playMode;
                _buffer[2] = (byte)rotationIndex;
                RaiseEvent(_buffer);
            }

            protected override void OnMsgReceived(byte[] payload)
            {
                _callback.Invoke(payload[1], payload[2]);
            }
        }
    }
}