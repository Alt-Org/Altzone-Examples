using Photon.Pun;
using UnityEngine;

namespace Examples2.Scripts.Battle.Player2
{
    /// <summary>
    /// Prefab for <c>Photon</c> to instantiate over network without any visual geometry.
    /// </summary>
    /// <remarks>
    /// Functional geometry is added later and this can be detached from it any time if required or when connection is lost etc. errors.
    /// </remarks>
    [RequireComponent(typeof(PhotonView))]
    public class PhotonPlayer : MonoBehaviour
    {
        [Header("Settings"), SerializeField] private PlayerActor2 _playerActorPrefab;

        private PlayerActor2 _playerActor;

        private void OnEnable()
        {
            if (_playerActor != null)
            {
                return;
            }
            Debug.Log($"OnEnable with {_playerActorPrefab.name}");
            name = nameof(PhotonPlayer);
            Debug.Log($"Instantiate");
            _playerActor = Instantiate(_playerActorPrefab);
            Debug.Log($"Re-parent");
            GetComponent<Transform>().parent = _playerActor.GetComponent<Transform>();
            Debug.Log($"SetActive");
            _playerActor.SetPhotonView(PhotonView.Get(this));
            _playerActor.gameObject.SetActive(true);
            Debug.Log($"OnEnable Done");
        }
    }
}