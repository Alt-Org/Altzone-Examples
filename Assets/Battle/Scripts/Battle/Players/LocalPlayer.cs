using Battle.Scripts.Battle.interfaces;
using Battle.Scripts.Battle.PlayerInput;
using Photon.Pun;
using UnityEngine;

namespace Battle.Scripts.Battle.Players
{
    /// <summary>
    /// Local player manager to handle input.
    /// </summary>
    [RequireComponent(typeof(PhotonView))]
    internal class LocalPlayer : MonoBehaviour
    {
        [SerializeField] private PlayerMovement _playerMovement;
        [SerializeField] private PlayerInput.PlayerInput _playerInput;
        [SerializeField] private PlayerInputKeyboard _playerInputKeyboard;

        private PhotonView _photonView;

        private void Awake()
        {
            _photonView = PhotonView.Get(this);

            _playerMovement = gameObject.AddComponent<PlayerMovement>();
            if (!_photonView.IsMine)
            {
                return;
            }
            // This is real local player that controls us.
            _playerInput = gameObject.AddComponent<PlayerInput.PlayerInput>();
            _playerInput.PlayerMovement = _playerMovement;
            _playerInputKeyboard = gameObject.AddComponent<PlayerInputKeyboard>();
            _playerInputKeyboard.PlayerMovement = _playerMovement;

            ((IMovablePlayer)_playerMovement).Speed = 7f;
        }
    }
}