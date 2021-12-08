using UnityEngine;
using UnityEngine.UI;

namespace GameUi.Scripts.MainMenu
{
    public class MainMenuView : MonoBehaviour
    {
        [SerializeField] private Button _testButtonA;
        [SerializeField] private Button _testButtonB;

        public Button TestButtonA => _testButtonA;
        public Button TestButtonB => _testButtonB;
    }
}