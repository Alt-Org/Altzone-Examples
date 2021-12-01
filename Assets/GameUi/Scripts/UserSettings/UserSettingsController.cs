using Altzone.Scripts.Config;
using Altzone.Scripts.Window;
using UnityEngine;

namespace GameUi.Scripts.UserSettings
{
    public class UserSettingsController : MonoBehaviour
    {
        [SerializeField] private UserSettingsView _view;

        private void Awake()
        {
            _view.SaveButton.onClick.AddListener(CheckButton);
            WindowManager.Get().RegisterGoBackHandlerOnce(CheckIfSaveSettings);
            var playerData = RuntimeGameConfig.Get().PlayerDataCache;
            _view.ShieldName.text = playerData.CharacterModel.Name;
            _view.PlayerNameInput.text = playerData.PlayerName;
        }

        private void CheckButton()
        {
            Debug.Log("CheckButton");
            CheckIfSaveSettings();
        }

        private WindowManager.GoBackAction CheckIfSaveSettings()
        {
            Debug.Log("CheckIfSaveSettings");
            var playerData = RuntimeGameConfig.Get().PlayerDataCache;
            var playerName = _view.PlayerNameInput.text;
            if (playerData.PlayerName != playerName)
            {
                Debug.Log($"PlayerName changed '{playerData.PlayerName}' <- '{playerName}'");
                playerData.PlayerName = playerName;
            }
            return WindowManager.GoBackAction.Continue;
        }
    }
}