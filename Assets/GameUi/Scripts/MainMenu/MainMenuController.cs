using System.Collections;
using System.Threading.Tasks;
using Altzone.Scripts.Config;
using GameUi.Scripts.ServiceTest;
using UnityEngine;

namespace GameUi.Scripts.MainMenu
{
    public class MainMenuController : MonoBehaviour
    {
        [SerializeField] private MainMenuView _view;

        private int _responseCounter;

        private void Awake()
        {
            _view.TestButtonA.onClick.AddListener(TestButtonA);
            _view.TestButtonB.onClick.AddListener(TestButtonB);
        }

        private void OnEnable()
        {
            var playerData = RuntimeGameConfig.Get().PlayerDataCache;
            Debug.Log(playerData.ToString());
            _view.PlayerInfo = $"{playerData.PlayerName} : {playerData.CharacterModel.Name}";
            _view.TestText = string.Empty;
        }

        private void OnDisable()
        {
            StopAllCoroutines();
        }

        private void TestButtonA()
        {
            _view.TestButtonA.interactable = true;
            Debug.Log("TestButtonA click start");
            var task = DemoServiceAsync.GetTimeInfo();
            StartCoroutine(WaitForResponse(task));
            Debug.Log("TestButtonA click end");
            _view.TestButtonA.interactable = true;
        }

        private IEnumerator WaitForResponse(Task<DemoServiceAsync.Response> task)
        {
            yield return new WaitUntil(() => task.IsCompleted);
            var response = task.Result;
            Debug.Log($"TestButtonA click response {response}");
            SetResponse(response);
        }

        private async void TestButtonB()
        {
            _view.TestButtonB.interactable = false;
            Debug.Log("TestButtonB click start");
            var response = await DemoServiceAsync.GetVersionInfo();
            Debug.Log($"TestButtonB click end response {response}");
            SetResponse(response);
            _view.TestButtonB.interactable = true;
        }

        private void SetResponse(DemoServiceAsync.Response response)
        {
            _responseCounter += 1;
            var responseText = response.Success ? response.Payload : response.Message;
            _view.TestText = $"{_responseCounter}: {responseText}";
        }
    }
}