using Altzone.Scripts.Config;
using Altzone.Scripts.Window;
using UnityEngine;

namespace GameUi.Scripts.UserSettings
{
    public class UserSettingsController : MonoBehaviour
    {
        [SerializeField] private UserSettingsView _view;

        private void OnEnable()
        {
            var playerData = RuntimeGameConfig.Get().PlayerDataCache;
            _view.PlayerInfo = $"{playerData.PlayerName} - {playerData.CharacterModel.Name}";
        }
    }
}