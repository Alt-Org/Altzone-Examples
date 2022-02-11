using System;
using Altzone.Scripts.Battle;
using Examples2.Scripts.Battle.interfaces;
using Examples2.Scripts.Battle.Players;
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

        private int _playMode;
        private int _rotationIndex;

        private Transform _shield;
        private SpriteRenderer _renderer;
        private Collider2D _collider;
        private int _playerPos;

        public PlayerShield2(ShieldConfig config, PhotonView photonView)
        {
            _config = config;
            var playerId = (byte)photonView.OwnerActorNr;
            _stateHelper = new ShieldStateHelper(PhotonEventDispatcher.Get(), MsgSetShield, playerId, OnSetShieldState);
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

        void IPlayerShield.SetShieldState(int playMode, int rotationIndex)
        {
            Debug.Log($"send SetShieldState {_playerPos} mode {playMode} rotation {rotationIndex}");
            _stateHelper.SetShieldState(playMode, rotationIndex);
        }

        void IPlayerShield.PlayHitEffects()
        {
            throw new NotImplementedException();
        }

        private void OnSetShieldState(int playMode, int rotationIndex)
        {
            if (rotationIndex >= _config.Shields.Length)
            {
                rotationIndex %= _config.Shields.Length;
            }
            Debug.Log($"OnSetShieldState {_playerPos} mode {_playMode} <- {playMode} rotation {_rotationIndex} <- {rotationIndex}");
            if (rotationIndex != _rotationIndex)
            {
                _shield.gameObject.SetActive(false);
                _rotationIndex = rotationIndex;
                _shield = _config.Shields[_rotationIndex];
                _shield.gameObject.SetActive(true);
                _renderer = _shield.GetComponent<SpriteRenderer>();
                _collider = _shield.GetComponent<Collider2D>();
            }
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

        private class ShieldStateHelper : AbstractPhotonEventHelper
        {
            private readonly Action<int, int> _callback;

            private readonly byte[] _buffer = new byte[1 + 1 + 1];

            public ShieldStateHelper(PhotonEventDispatcher photonEventDispatcher, byte msgId, byte playerId, Action<int, int> onSetShieldState)
                : base(photonEventDispatcher, msgId, playerId)
            {
                _callback = onSetShieldState;
                _buffer[0] = playerId;
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