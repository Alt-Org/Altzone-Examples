using Examples2.Scripts.Battle.interfaces;
using Examples2.Scripts.Battle.Players;
using UnityEngine;

namespace Examples2.Scripts.Battle.Players2
{
    internal class PlayerShield2 : IPlayerShield2
    {
        private static readonly string[] StateNames = { "Norm", "Frozen", "Ghost" };

        private readonly ShieldConfig _config;

        private int _playerPos;
        private bool _isVisible;
        private int _playMode;
        private int _rotationIndex;

        private GameObject _shield;
        private Collider2D _collider;
        private ParticleSystem _particleEffect;

        public bool IsVisible => _isVisible;
        public int RotationIndex => _rotationIndex;

        public string StateString => $"{(_isVisible ? "V" : "H")} R{_rotationIndex} {(_collider.enabled ? "col" : "~~~")}";

        public PlayerShield2(ShieldConfig config)
        {
            _config = config;
        }

        private void Setup(bool isLower)
        {
            _particleEffect = _config._particle;
            var shields = _config.Shields;
            var isShieldRotated = !isLower;
            for (var i = 0; i < shields.Length; ++i)
            {
                var shield = shields[i];
                if (isShieldRotated)
                {
                    shield.Rotate(true);
                }
                shield.name = $"{_playerPos}:{shield.name}";
                if (i == _rotationIndex)
                {
                    _shield = shield.gameObject;
                    _shield.SetActive(true);
                    _collider = shield.GetComponent<Collider2D>();
                }
                else
                {
                    shield.gameObject.SetActive(false);
                }
            }
        }

        void IPlayerShield2.Setup(int playerPos, bool isLower, bool isVisible, int playMode, int rotationIndex)
        {
            _playerPos = playerPos;
            Setup(isLower);
            ((IPlayerShield2)this).SetVisibility(isVisible);
            ((IPlayerShield2)this).SetPlayMode(playMode);
            ((IPlayerShield2)this).SetRotation(rotationIndex);
        }

        void IPlayerShield2.SetVisibility(bool isVisible)
        {
            _isVisible = isVisible;
            _shield.SetActive(_isVisible);
        }

        void IPlayerShield2.SetPlayMode(int playMode)
        {
            _playMode = playMode;
            Debug.Log(
                $"SetShieldState {_playerPos} mode {StateNames[_playMode]} <- {StateNames[playMode]} rotation {_rotationIndex} collider {_collider.enabled}");
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
            _shield.SetActive(_isVisible);
        }

        void IPlayerShield2.SetRotation(int rotationIndex)
        {
            if (rotationIndex >= _config.Shields.Length)
            {
                rotationIndex %= _config.Shields.Length;
            }
            Debug.Log($"OnSetShieldRotation {_playerPos} mode {StateNames[_playMode]} rotation {_rotationIndex} <- {rotationIndex}");
            _rotationIndex = rotationIndex;
            _shield.SetActive(false);
            _shield = _config.Shields[_rotationIndex].gameObject;
            _collider = _shield.GetComponent<Collider2D>();
            _shield.SetActive(_isVisible);
        }

        void IPlayerShield2.PlayHitEffects(Vector2 contactPoint)
        {
            _particleEffect.Play();
#if UNITY_EDITOR
            UnityEngine.Debug.DrawLine(Vector3.zero, contactPoint, Color.magenta, 1f);
#endif
        }
    }
}
