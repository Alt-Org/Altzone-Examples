using Examples.Config.Scripts;
using Photon.Pun;
using UnityEngine;

namespace Examples.Game.Scripts.Battle.Player
{
    public interface IPlayerShield
    {
        void showShield();
        void hideShield();
    }

    /// <summary>
    /// Controls player shield and synchronizes its state over network using <c>RPC</c>.
    /// </summary>
    /// <remarks>
    /// Shield can be visible or hidden and it can be "bent" when gets hit by the ball.
    /// </remarks>
    public class PlayerShield : MonoBehaviour, IPlayerShield
    {
        [Header("Settings"), SerializeField] private GameObject upperShield;
        [SerializeField] private GameObject lowerShield;

        [Header("Live Data"), SerializeField] protected PhotonView _photonView;
        [SerializeField] protected Transform _transform;
        [SerializeField] protected PlayerShield _otherPlayerShield;
        [SerializeField] protected Transform _otherTransform;
        [SerializeField] private GameObject currentShield;
        [SerializeField] private float sqrShieldDistance;
        [SerializeField] private float sqrDistance;
        [SerializeField] private bool isShieldVisible;
        [SerializeField] private bool isShieldActive;

        [Header("Debug"), SerializeField] private bool isDebug;

        private void Awake()
        {
            _photonView = PhotonView.Get(this);
            _transform = GetComponent<Transform>();
            sqrShieldDistance = RuntimeGameConfig.Get().variables.shieldDistance * 2f;
            upperShield.SetActive(false);
            lowerShield.SetActive(false);
            isShieldVisible = false;
            isShieldActive = false;
            enabled = false;
        }

        private void OnEnable()
        {
            var playerActor = GetComponent<PlayerActor>() as IPlayerActor;
            currentShield = playerActor.IsLocalTeam ? upperShield : lowerShield;
            Debug.Log($"OnEnable {name} IsLocalTeam={playerActor.IsLocalTeam} currentShield={currentShield.name}");
            var teamMate = playerActor.TeamMate;
            if (teamMate == null)
            {
                if (isDebug && _otherTransform != null)
                {
                    return; // _otherTransform has been set in Editor!
                }
                enabled = false;
                return;
            }
            _otherTransform = ((PlayerActor)teamMate).transform;
            _otherPlayerShield = _otherTransform.GetComponent<PlayerShield>();
            Debug.Log($"OnEnable {name} teamMate={_otherTransform.gameObject.name} shield={currentShield.name}");
        }

        private void OnDisable()
        {
            Debug.Log($"OnDisable name={name}");
            if (_otherPlayerShield != null)
            {
                // Must disable our team mate - for now
                _otherPlayerShield.enabled = false;
            }
        }

        private void Update()
        {
            sqrDistance = Mathf.Abs((_transform.position - _otherTransform.position).sqrMagnitude);
            if (sqrDistance < sqrShieldDistance)
            {
                if (!isShieldVisible)
                {
                    Debug.Log($"Show shield {currentShield.name}");
                    isShieldVisible = true;
                    if (isShieldActive)
                    {
                        currentShield.SetActive(true);
                    }
                }
            }
            else
            {
                if (isShieldVisible)
                {
                    Debug.Log($"Hide shield {currentShield.name}");
                    isShieldVisible = false;
                    currentShield.SetActive(false);
                }
            }
        }

        void IPlayerShield.showShield()
        {
            isShieldActive = true;
            if (isShieldVisible)
            {
                currentShield.SetActive(true);
            }
        }

        void IPlayerShield.hideShield()
        {
            isShieldActive = false;
            if (isShieldVisible)
            {
                currentShield.SetActive(false);
            }
        }
    }
}