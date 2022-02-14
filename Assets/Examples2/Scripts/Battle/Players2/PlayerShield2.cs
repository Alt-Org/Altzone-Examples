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
        private static readonly string[] StateNames = { "Norm", "Frozen", "Ghost" };

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
            _playMode = 0;
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
            Debug.Log($"SetShieldState {_playerPos} mode {StateNames[_playMode]} <- {StateNames[playMode]} rotation {_rotationIndex} collider {_collider.enabled}");
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

        void IPlayerShield.SetShieldRotation(int rotationIndex, Vector2 contactPoint)
        {
            Debug.Log($"SetShieldRotation {_playerPos} mode {StateNames[_playMode]} rotation {_rotationIndex} <- {rotationIndex}");
            SendShieldRotationRpc(rotationIndex, contactPoint);
        }

        private void PlayHitEffects(Vector2 contactPoint)
        {
#if UNITY_EDITOR
            UnityEngine.Debug.DrawLine(Vector3.zero, contactPoint, Color.magenta, 1f);
#endif
        }

        private void SetShieldRotation(int rotationIndex, Vector2 contactPoint)
        {
            if (rotationIndex >= _config.Shields.Length)
            {
                rotationIndex %= _config.Shields.Length;
            }
            Debug.Log($"OnSetShieldRotation {_playerPos} mode {StateNames[_playMode]} rotation {_rotationIndex} <- {rotationIndex} @ {contactPoint}");
            if (rotationIndex != _rotationIndex)
            {
                _shield.gameObject.SetActive(false);
                _rotationIndex = rotationIndex;
                _shield = _config.Shields[_rotationIndex];
                _shield.gameObject.SetActive(true);
                _collider = _shield.GetComponent<Collider2D>();
                ((IPlayerShield)this).SetShieldState(_playMode);
            }
            PlayHitEffects(contactPoint);
        }

        #region Photon Event (RPC Message) Marshalling

        private readonly byte[] _setShieldRotationMsgBuffer = new byte[1 + 1 + 4 + 4];

        private byte[] SetShieldRotationBytes(int rotationIndex, Vector2 contactPoint)
        {
            var index = 1;
            _setShieldRotationMsgBuffer[index] = (byte)rotationIndex;
            index += 1;
            Array.Copy(BitConverter.GetBytes(contactPoint.x), 0, _setShieldRotationMsgBuffer, index, 4);
            index += 4;
            Array.Copy(BitConverter.GetBytes(contactPoint.y), 0, _setShieldRotationMsgBuffer, index, 4);

            return _setShieldRotationMsgBuffer;
        }

        /// <summary>
        /// Naming convention to send message over networks is Send-ShieldRotation-Rpc
        /// </summary>
        private void SendShieldRotationRpc(int rotationIndex, Vector2 contactPoint)
        {
            _photonEvent.SendEvent(MsgSetShieldRotation, SetShieldRotationBytes(rotationIndex, contactPoint));
        }

        /// <summary>
        /// Naming convention to receive message from networks is On-ShieldRotation-Callback
        /// </summary>
        private void OnSetShieldRotationCallback(byte[] payload)
        {
            var index = 1;
            var rotationIndex = payload[index];
            Vector2 contactPoint;
            index += 1;
            contactPoint.x = BitConverter.ToSingle(payload, index);
            index += 4;
            contactPoint.y = BitConverter.ToSingle(payload, index);

            SetShieldRotation(rotationIndex, contactPoint);
        }

        #endregion
    }
}