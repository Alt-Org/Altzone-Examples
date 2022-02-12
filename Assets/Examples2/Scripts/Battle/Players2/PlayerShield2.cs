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
        private readonly ShieldRotationHelper _rotationHelper;

        private int _playMode;
        private int _rotationIndex;

        private Transform _shield;
        private SpriteRenderer _renderer;
        private Collider2D _collider;
        private int _playerPos;

        public string StateString => $"R{_rotationIndex} {(_collider.enabled ? "col" : "~~~")}";

        public PlayerShield2(ShieldConfig config, PhotonView photonView)
        {
            _config = config;
            var playerId = (byte)photonView.OwnerActorNr;
            _rotationHelper = new ShieldRotationHelper(PhotonEventDispatcher.Get(), MsgSetShieldRotation, playerId, OnSetShieldRotation);
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
                    _renderer = renderer;
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
            _rotationHelper.SetShieldRotation(rotationIndex);
        }

        void IPlayerShield.PlayHitEffects()
        {
            throw new NotImplementedException();
        }

        private void OnSetShieldRotation(int rotationIndex)
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
                _renderer = _shield.GetComponent<SpriteRenderer>();
                _collider = _shield.GetComponent<Collider2D>();
            }
        }

        private class ShieldRotationHelper : AbstractPhotonEventHelper
        {
            private readonly Action<int> _callback;

            private readonly byte[] _buffer = new byte[1 + 1];

            public ShieldRotationHelper(PhotonEventDispatcher photonEventDispatcher, byte msgId, byte playerId, Action<int> onSetShieldRotation)
                : base(photonEventDispatcher, msgId, playerId)
            {
                _callback = onSetShieldRotation;
                _buffer[0] = playerId;
            }

            public void SetShieldRotation(int rotationIndex)
            {
                _buffer[1] = (byte)rotationIndex;
                RaiseEvent(_buffer);
            }

            protected override void OnMsgReceived(byte[] payload)
            {
                _callback.Invoke(payload[1]);
            }
        }
    }
}