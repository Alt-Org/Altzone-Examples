using Altzone.Scripts.Config;
using UnityEngine;

namespace GameUi.Scripts.PlayerName
{
    public class PlayerNameController : MonoBehaviour
    {
        [SerializeField] private PlayerNameView _view;

        private void Awake()
        {
            var playerData = RuntimeGameConfig.GetPlayerDataCacheInEditor();
            Debug.Log(playerData.ToString());
            _view.PlayerName = playerData.PlayerName;

            //_view.ContinueButton.interactable = !string.IsNullOrWhiteSpace(_view.PlayerName);
            _view.ContinueButton.onClick.AddListener(ContinueButton);
        }

        private void ContinueButton()
        {
            var playerData = RuntimeGameConfig.GetPlayerDataCacheInEditor();
            if (_view.PlayerName != playerData.PlayerName)
            {
                playerData.BatchSave(() =>
                {
                    playerData.PlayerName = _view.PlayerName;
                });
                Debug.Log(playerData.ToString());
            }
        }
    }
}
