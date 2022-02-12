using Altzone.Scripts.Config;
using Prg.Scripts.Common.Unity.Localization;
using UnityEngine;

namespace GameUi.Scripts.UserSettings
{
    public class UserSettingsController : MonoBehaviour
    {
        [SerializeField] private UserSettingsView _view;

        private void OnEnable()
        {
            var playerData = RuntimeGameConfig.Get().PlayerDataCache;
            Debug.Log($"OnEnable {playerData}");
            _view.PlayerInfo = playerData.GetPlayerInfoLabel();
            if (playerData.ClanId > 0)
            {
                _view.ShowLeaveClanButton();
            }
            else
            {
                _view.ShowJoinClanButton();
            }
            // Manual localization to amend button caption.
            var localizationKeyName = _view.ToggleDebug.GetComponentsInChildren<LocalizationKeyName>(true)[0];
            var originalCaption = Localizer.Localize(localizationKeyName.LocalizationKey);
            var toggleDebugCaption = $"{originalCaption}: {(playerData.IsDebugFlag ? "1" : "0")}";
            _view.ToggleDebug.SetCaption(toggleDebugCaption);
            _view.ToggleDebug.onClick.AddListener(() =>
            {
                playerData.BatchSave(() => { playerData.IsDebugFlag = !playerData.IsDebugFlag; });
                toggleDebugCaption = $"{originalCaption}: {(playerData.IsDebugFlag ? "1" : "0")}";
                _view.ToggleDebug.SetCaption(toggleDebugCaption);
            });
        }
    }
}