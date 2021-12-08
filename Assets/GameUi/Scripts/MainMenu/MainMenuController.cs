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
            _view.TestButtonA.onClick.AddListener(TestButtonA);
            _view.TestButtonB.onClick.AddListener(TestButtonB);
        }

        private void OnEnable()
        {
            Assert.IsNotNull(_view, "_view != null");
            var playerData = RuntimeGameConfig.Get().PlayerDataCache;
            Debug.Log(playerData.ToString());
        }

        private void TestButtonA()
        {
            _view.TestButtonA.interactable = true;
            Debug.Log("TestButtonA click");
            _view.TestButtonA.interactable = true;
        }

        private async void TestButtonB()
        {
            // This is a blocking call - UNITY main thread is blocked by this call!
            _view.TestButtonB.interactable = false;
            Debug.Log("TestButtonB click");
            const string serviceUrl = "https://jsonplaceholder.typicode.com/";
            var service = new DemoServiceAsync(serviceUrl);
            var response = await service.GetVersionInfo("todos/1") ?? string.Empty;
            Debug.Log($"TestButtonB response {response.Replace("\r", "").Replace("\n", "")}");
            _view.TestButtonB.interactable = true;
        }
    }
}