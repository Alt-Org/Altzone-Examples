using System;
using Altzone.Scripts.Battle;
using Photon.Pun;
using UnityEngine;

namespace Examples2.Scripts.Battle.Players
{
    [Serializable]
    public class Shield
    {
        [SerializeField] private Transform _pivot;
        [SerializeField] private SpriteRenderer _sprite;
        [SerializeField] private Collider2D _collider;

        public void Set(Color spriteColor, bool enabledState)
        {
            _sprite.color = spriteColor;
            _collider.enabled = enabledState;
        }

        public void Rotate(float degrees)
        {
            _pivot.rotation = Quaternion.identity;
            _pivot.Rotate(0, 0, degrees);
        }
    }

    public class PlayerShield : MonoBehaviour
    {
        [Header("Settings"), SerializeField] private Transform _shieldPivot;
        [SerializeField] private Shield _leftShield;
        [SerializeField] private Shield _rightShield;

        private void Awake()
        {
            var photonView = PhotonView.Get(this);
            var player = photonView.Owner;
            var playerPos = PhotonBattle.GetPlayerPos(player);
            var teamNumber = PhotonBattle.GetTeamNumber(playerPos);
            if (teamNumber == PhotonBattle.TeamRedValue)
            {
                // Flip shield from head to toes
                var localPosition = _shieldPivot.localPosition;
                localPosition.y = -localPosition.y;
                _shieldPivot.localPosition = localPosition;
            }
            SetShields(PlayerActor.PlayModeGhosted);
            RotateShields(0);
        }

        public void SetShields(int playMode)
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

        public void RotateShields(float degrees)
        {
            _leftShield.Rotate(degrees);
            _rightShield.Rotate(-degrees);
        }
    }
}