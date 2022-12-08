using Altzone.Scripts.Config;
using UnityEngine;

namespace GameUi.Scripts.CharacterSelection
{
    public class CharacterSelectionController : MonoBehaviour
    {
        [SerializeField] private CharacterSelectionView _view;

        private void OnEnable()
        {
            var playerData = GameConfig.Get().PlayerDataCache;
            _view.PlayerInfo = playerData.PlayerName;
            _view.ShowCharacter(playerData.CustomCharacterModelId);

            _view.OnCharacterSelectionChanged += OnCharacterSelectionChanged;
        }

        private void OnDisable()
        {
            _view.OnCharacterSelectionChanged -= OnCharacterSelectionChanged;
        }

        private void OnCharacterSelectionChanged(int characterModelId)
        {
            var playerData = GameConfig.Get().PlayerDataCache;
            if (playerData.CustomCharacterModelId != characterModelId)
            {
                playerData.SetCustomCharacterModelId(characterModelId);
            }
            _view.ShowCharacter(characterModelId);
        }
    }
}