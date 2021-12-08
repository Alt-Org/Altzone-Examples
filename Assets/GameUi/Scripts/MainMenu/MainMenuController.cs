using System.Collections;
using System.Threading.Tasks;
using Altzone.Scripts.Config;
using GameUi.Scripts.ServiceTest;
using UnityEngine;
using UnityEngine.Assertions;

namespace GameUi.Scripts.MainMenu
{
    public class MainMenuController : MonoBehaviour
    {
        [SerializeField] private MainMenuView _view;

        private readonly DemoServiceAsync _service = new DemoServiceAsync();
        private int _responseCounter;

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
            _view.PlayerInfo = $"{playerData.PlayerName} : {playerData.CharacterModel.Name}";
            _view.TestText = string.Empty;
        }

        private void TestButtonA()
        {
            _view.TestButtonA.interactable = true;
            Debug.Log("TestButtonA click start");
            var task = _service.GetVersionInfo();
            StartCoroutine(WaitForResponse(task));
            Debug.Log("TestButtonA click end");
            _view.TestButtonA.interactable = true;
        }

        private IEnumerator WaitForResponse(Task<string> task)
        {
            yield return new WaitUntil(() => task.IsCompleted);
            var response = task.Result;
            Debug.Log($"TestButtonA click response {response}");
            _responseCounter += 1;
            if (response.Length > 20)
            {
                response = response.Substring(0, 20);
            }
            _view.TestText = $"{_responseCounter}: {response}";
        }

        private async void TestButtonB()
        {
            _view.TestButtonB.interactable = false;
            Debug.Log("TestButtonB click start");
            var response = await _service.GetVersionInfo();
            Debug.Log($"TestButtonB click end response {response}");
            _view.TestButtonB.interactable = true;
            _responseCounter += 1;
            if (response.Length > 20)
            {
                response = response.Substring(0, 20);
            }
            _view.TestText = $"{_responseCounter}: {response}";
        }
    }
}