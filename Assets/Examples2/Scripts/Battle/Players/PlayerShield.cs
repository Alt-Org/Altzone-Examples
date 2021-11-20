using System;
using Altzone.Scripts.Battle;
using UnityEngine;

namespace Examples2.Scripts.Battle.Players
{
    /// <summary>
    /// Helper class for shield part (left or right).
    /// </summary>
    [Serializable]
    public class Shield
    {
        [Header("Settings"), SerializeField] private Transform _pivot;
        [SerializeField] private SpriteRenderer _sprite;
        [SerializeField] private Collider2D _collider;

        [Header("Live Data"), SerializeField] private float _rotationDirection;

        public float RotationDirection
        {
            set => _rotationDirection = value;
        }

        public void Set(Color spriteColor, bool enabledState)
        {
            _sprite.color = spriteColor;
            _collider.enabled = enabledState;
        }

        public void Rotate(float degrees)
        {
            _pivot.rotation = Quaternion.identity;
            _pivot.Rotate(0, 0, _rotationDirection * degrees);
        }
    }

    /// <summary>
    /// Manages player shield visible attributes.
    /// </summary>
    public class PlayerShield : MonoBehaviour
    {
        [Header("Settings"), SerializeField] private Transform _shieldPivot;
        [SerializeField] private Shield _leftShield;
        [SerializeField] private Shield _rightShield;

        private void Awake()
        {
            _leftShield.RotationDirection = 1f;
            _rightShield.RotationDirection = -1f;
        }

        public void SetShieldSide(int teamNumber)
        {
            // Set shield side: "head" or "toes" relative to our origo.
            var localPosition = _shieldPivot.localPosition;
            var side = teamNumber == PhotonBattle.TeamBlueValue ? 1f : -1f;
            localPosition.y = side * localPosition.y;
            _shieldPivot.localPosition = localPosition;
        }

        public void SetShieldMode(int playMode)
        {
            switch (playMode)
            {
                case PlayerActor.PlayModeNormal:
                case PlayerActor.PlayModeFrozen:
                    _leftShield.Set(Color.white, true);
                    _rightShield.Set(Color.white, true);
                    break;
                default:
                    _leftShield.Set(Color.grey, false);
                    _rightShield.Set(Color.grey, false);
                    break;
            }
        }

        public void SetShieldRotation(float degrees)
        {
            _leftShield.Rotate(degrees);
            _rightShield.Rotate(degrees);
        }
    }
}