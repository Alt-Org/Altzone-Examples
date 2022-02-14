using System;
using Examples2.Scripts.Battle.interfaces;
using Examples2.Scripts.Battle.Players;
using Photon.Pun;
using Prg.Scripts.Common.Photon;
using UnityEngine;

namespace Examples2.Scripts.Battle.Players2
{
    internal class PlayerShield2 : IPlayerShield
    {
        private const byte MsgSetShieldRotation = PhotonEventDispatcher.EventCodeBase + 7;

        private readonly ShieldConfig _config;
        private readonly PhotonEventHelper _photonEvent;

        private int _playMode;
        private int _rotationIndex;

        private Transform _shield;
        private Collider2D _collider;
        private int _playerPos;

        public string StateString => $"R{_rotationIndex} {(_collider.enabled ? "col" : "~~~")}";

        public PlayerShield2(ShieldConfig config, PhotonView photonView)
        {
            _config = config;
            var playerId = (byte)photonView.OwnerActorNr;
            _photonEvent = new PhotonEventHelper(PhotonEventDispatcher.Get(), playerId);
            _photonEvent.RegisterEvent(MsgSetShieldRotation, OnSetShieldRotationCallback);
        }

        void IPlayerShield.SetupShield(int playerPos, bool isLower)
        {
            Debug.Log($"SetupShield playerPos {playerPos}");
            _playMode = -1;
            _rotationIndex = 0;
            _playerPos = playerPos;
            var shields = _config.Shields;
            var isShieldFlipped = isLower;
            for (var i = 0; i < shields.Length; ++i)
            {
                var shield = shields[i];
                shield.name = $"{playerPos}:{shield.name}";
                var renderer = shield.GetComponent<SpriteRenderer>();
                renderer.flipY = isShieldFlipped;
                if (i == _rotationIndex)
                {
                    _shield = shield;
                    _shield.gameObject.SetActive(true);
                    _collider = shield.GetComponent<Collider2D>();
                }
                else
                {
                    shield.gameObject.SetActive(false);
                }
            }
        }

        void IPlayerShield.SetShieldState(int playMode)
        {
            Debug.Log($"SetShieldState {_playerPos} mode {playMode}");
            if (playMode != _playMode)
            {
                _playMode = playMode;
                switch (_playMode)
                {
                    case PlayerActor.PlayModeNormal:
                    case PlayerActor.PlayModeFrozen:
                        _collider.enabled = true;
                        break;
                    case PlayerActor.PlayModeGhosted:
                        _collider.enabled = false;
                        break;
                    default:
                        throw new UnityException($"invalid playmode {_playMode}");
                }
            }
        }

        void IPlayerShield.SetShieldRotation(int rotationIndex)
        {
            Debug.Log($"SetShieldRotation {_playerPos} mode {_playMode} rotation {rotationIndex}");
            SendShieldRotationRpc(rotationIndex);
        }

        void IPlayerShield.PlayHitEffects()
        {
            throw new NotImplementedException();
        }

        private void SetShieldRotation(int rotationIndex)
        {
            if (rotationIndex >= _config.Shields.Length)
            {
                rotationIndex %= _config.Shields.Length;
            }
            Debug.Log($"OnSetShieldRotation {_playerPos} mode {_playMode} rotation {_rotationIndex} <- {rotationIndex}");
            if (rotationIndex != _rotationIndex)
            {
                _shield.gameObject.SetActive(false);
                _rotationIndex = rotationIndex;
                _shield = _config.Shields[_rotationIndex];
                _shield.gameObject.SetActive(true);
                _collider = _shield.GetComponent<Collider2D>();
            }
        }

        #region Photon Event (RPC Message) Marshalling

        private readonly byte[] _setShieldRotationMsgBuffer = new byte[1 + 1];

        private byte[] SetShieldRotationBytes(int rotationIndex)
        {
            _setShieldRotationMsgBuffer[1] = (byte)rotationIndex;

            return _setShieldRotationMsgBuffer;
        }

        /// <summary>
        /// Naming convention to send message over networks is Send-ShieldRotation-Rpc
        /// </summary>
        private void SendShieldRotationRpc(int rotationIndex)
        {
            _photonEvent.SendEvent(MsgSetShieldRotation, SetShieldRotationBytes(rotationIndex));
        }

        /// <summary>
        /// Naming convention to receive message from networks is On-ShieldRotation-Callback
        /// </summary>
        private void OnSetShieldRotationCallback(byte[] payload)
        {
            var rotationIdex = payload[1];

            SetShieldRotation(rotationIdex);
        }

        #endregion
    }
}