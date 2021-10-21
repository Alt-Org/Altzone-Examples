using Examples.Config.Scripts;
using Photon.Pun;
using System;
using System.Collections;
using UnityEngine;

namespace Examples.Game.Scripts.Battle.Player
{
    /// <summary>
    /// Controls player shield and synchronizes its state over network using <c>RPC</c>.
    /// </summary>
    /// <remarks>
    /// Shield can be visible or hidden and it can be "bent" when gets hit by the ball.
    /// </remarks>
    public class PlayerShield : MonoBehaviour
    {
        [Header("Settings"), SerializeField] private GameObject upperShield;
        [SerializeField] private GameObject lowerShield;

        [Header("Live Data"), SerializeField] protected PhotonView _photonView;
        [SerializeField] protected Transform _transform;
        [SerializeField] protected Transform _otherTransform;
        [SerializeField] private GameObject currentShield;
        [SerializeField] private float sqrShieldDistance;
        [SerializeField] private float sqrDistance;
        [SerializeField] private bool isShieldVisible;

        [Header("Debug"), SerializeField] private bool isDebug;

        private void Awake()
        {
            _photonView = PhotonView.Get(this);
            _transform = GetComponent<Transform>();
            sqrShieldDistance = RuntimeGameConfig.Get().variables.shieldDistance * 2f;
        }

        private void OnEnable()
        {
            Debug.Log($"OnEnable allPlayerActors={PlayerActor.allPlayerActors.Count}");
            var playerActor = GetComponent<PlayerActor>() as IPlayerActor;
            currentShield = playerActor.IsLocalTeam ? upperShield : lowerShield;
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
            Debug.Log($"OnEnable teamMate={_otherTransform.gameObject.name} shield={currentShield.name}");
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
                    currentShield.SetActive(true);
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
    }
}