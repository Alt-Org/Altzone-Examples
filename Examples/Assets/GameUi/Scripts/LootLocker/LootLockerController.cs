using Altzone.Scripts.Service.LootLocker;
using UnityEngine;

namespace GameUi.Scripts.LootLocker
{
    public class LootLockerController : MonoBehaviour
    {
        private const string WaitText = "Wait";

        [SerializeField] private LootLockerView _view;

        private void OnEnable()
        {
            _view.Reset();
            if (!LootLockerWrapper.IsRunning)
            {
                _view.InfoLabel = "LootLocker is not running";
                return;
            }
            _view.InfoLabel = $"{LootLockerWrapper.PlayerName}";

            var button1 = _view.TestButton1;
            button1.SetCaption("Ping");
            button1.onClick.AddListener(TestButton1);

            var button2 = _view.TestButton2;
            button2.SetCaption("Chars");
            var charactersTest = GetComponent<CharactersTest>();
            button2.onClick.AddListener(() => charactersTest.enabled = !charactersTest.enabled);
        }

        private async void TestButton1()
        {
            var button = _view.TestButton1;
            button.interactable = false;
            _view.ResultLabel = WaitText;
            var result = await LootLockerWrapper.PingAsync();
            _view.ResultLabel = result;
            button.interactable = true;
        }
    }
}