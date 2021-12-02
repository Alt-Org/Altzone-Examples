using UnityEngine;
using UnityEngine.UI;

namespace GameUi.Scripts.BattleLobby
{
    public class BattleLobbyView : MonoBehaviour
    {
        [SerializeField] private Button _startGameButton;

        public Button StartGameButton => _startGameButton;
    }
}