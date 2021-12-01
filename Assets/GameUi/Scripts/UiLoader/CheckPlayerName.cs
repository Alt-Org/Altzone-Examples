using Altzone.Scripts.Config;
using Altzone.Scripts.Window;
using Altzone.Scripts.Window.ScriptableObjects;
using UnityEngine;
using UnityEngine.UI;

namespace GameUi.Scripts.UiLoader
{
    public class CheckPlayerName : MonoBehaviour
    {
        [SerializeField] private WindowDef _nextWindow;
        [SerializeField] private GameObject _window;
        [SerializeField] private Button _button;
        [SerializeField] private bool _isSkipValidName;

        private void OnEnable()
        {
            var playerData = RuntimeGameConfig.Get().PlayerDataCache;
            Debug.Log($"PlayerName '{playerData.PlayerName}'");
            if (_isSkipValidName && !string.IsNullOrWhiteSpace(playerData.PlayerName))
            {
                WindowManager.Get().ShowWindow(_nextWindow);
                return;
            }
            _window.SetActive(true);

            _button.onClick.AddListener(() =>
            {
                Debug.Log("continueButton");
                WindowManager.Get().ShowWindow(_nextWindow);
            });
        }
    }
}