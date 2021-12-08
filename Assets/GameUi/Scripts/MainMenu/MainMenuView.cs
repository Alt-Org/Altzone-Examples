using UnityEngine;
using UnityEngine.UI;

namespace GameUi.Scripts.MainMenu
{
    public class MainMenuView : MonoBehaviour
    {
        [SerializeField] private Button _testButton;

        public Button TestButton => _testButton;
    }
}