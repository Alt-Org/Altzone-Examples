using Altzone.Scripts.Battle;
using Photon.Pun;
using UnityEngine;

namespace Examples2.Scripts.Battle.Players
{
    public class PlayerShield : MonoBehaviour
    {
        [Header("Settings"), SerializeField] private Transform _shieldPivot;
        [SerializeField] private SpriteRenderer _leftShield;
        [SerializeField] private SpriteRenderer _rightShield;

        private Collider2D _leftCollider;
        private Collider2D _rightCollider;

        private void Awake()
        {
            _leftCollider = _leftShield.GetComponent<Collider2D>();
            _rightCollider = _rightShield.GetComponent<Collider2D>();
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
        }

        public void SetShields(int playMode)
        {
            switch (playMode)
            {
                case PlayerActor.PlayModeNormal:
                case PlayerActor.PlayModeFrozen:
                    _leftShield.color = Color.white;
                    _rightShield.color = Color.white;
                    _leftCollider.enabled = true;
                    _rightCollider.enabled = true;
                    break;
                default:
                    _leftShield.color = Color.grey;
                    _rightShield.color = Color.grey;
                    _leftCollider.enabled = false;
                    _rightCollider.enabled = false;
                    break;
            }
        }
    }
}