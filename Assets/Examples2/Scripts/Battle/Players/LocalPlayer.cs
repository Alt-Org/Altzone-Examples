using Examples2.Scripts.Battle.interfaces;
using Examples2.Scripts.Battle.PlayerInput;
using UnityEngine;

namespace Examples2.Scripts.Battle.Players
{
    /// <summary>
    /// Local player manager to handle input.
    /// </summary>
    public class LocalPlayer : MonoBehaviour
    {
        [SerializeField] private PlayerMovement _playerMovement;
        [SerializeField] private PlayerInput.PlayerInput _playerInput;
        [SerializeField] private PlayerInputKeyboard _playerInputKeyboard;

        private void Awake()
        {
            _playerMovement = gameObject.AddComponent<PlayerMovement>();
            _playerInput = gameObject.AddComponent<PlayerInput.PlayerInput>();
            _playerInput.PlayerMovement = _playerMovement;
            _playerInputKeyboard = gameObject.AddComponent<PlayerInputKeyboard>();
            _playerInputKeyboard.PlayerMovement = _playerMovement;

            ((IMovablePlayer)_playerMovement).Speed = 7f;
        }
    }
}