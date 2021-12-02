using UnityEngine;

namespace GameUi.Scripts.BattleLobby
{
    public class BattleLobbyController : MonoBehaviour
    {
        [SerializeField] private BattleLobbyView _view;

        private void Awake()
        {
            _view.StartGameButton.onClick.AddListener(StartGameButton);
        }

        private void StartGameButton()
        {
            Debug.Log("StartGameButton");
        }
    }
}