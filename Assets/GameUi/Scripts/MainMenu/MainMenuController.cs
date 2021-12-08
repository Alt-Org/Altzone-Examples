using Altzone.Scripts.Config;
using GameUi.Scripts.ServiceTest;
using UnityEngine;
using UnityEngine.Assertions;

namespace GameUi.Scripts.MainMenu
{
    public class MainMenuController : MonoBehaviour
    {
        [SerializeField] private MainMenuView _view;

        private void Awake()
        {
            _view.TestButton.onClick.AddListener(TestButton);
        }

        private void OnEnable()
        {
            Assert.IsNotNull(_view, "_view != null");
            var playerData = RuntimeGameConfig.Get().PlayerDataCache;
            Debug.Log(playerData.ToString());
        }

        private static async void TestButton()
        {
            // This is blocking call!
            Debug.Log("TestButton");
            const string serviceUrl = "https://jsonplaceholder.typicode.com/";
            var service = new DemoServiceAsync(serviceUrl);
            var response = await service.GetVersionInfo("todos/1") ?? string.Empty;
            Debug.Log($"response {response.Replace("\r", "").Replace("\n", "")}");
        }
    }
}