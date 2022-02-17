using Altzone.Scripts.Config;
using UnityEngine;

namespace GameUi.Scripts.CharacterSelection
{
    public class CharacterSelectionController : MonoBehaviour
    {
        [SerializeField] private CharacterSelectionView _view;

        private void OnEnable()
        {
            var playerData = RuntimeGameConfig.Get().PlayerDataCache;
            _view.PlayerInfo = playerData.PlayerName;
            _view.ShowCharacter(playerData.CharacterModelId);

            _view.OnCharacterSelectionChanged += OnCharacterSelectionChanged;
        }

        private void OnDisable()
        {
            _view.OnCharacterSelectionChanged -= OnCharacterSelectionChanged;
        }

        private void OnCharacterSelectionChanged(int characterModelId)
        {
            var playerData = RuntimeGameConfig.Get().PlayerDataCache;
            if (playerData.CharacterModelId != characterModelId)
            {
                playerData.BatchSave(() => { playerData.CharacterModelId = characterModelId; });
            }
            _view.ShowCharacter(characterModelId);
        }
    }
}