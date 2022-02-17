using System;
using System.Collections.Generic;
using Altzone.Scripts.Model;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.PlayerLoop;
using UnityEngine.UI;

namespace GameUi.Scripts.CharacterSelection
{
    public class CharacterSelectionView : MonoBehaviour
    {
        [SerializeField] private Text _playerInfo;
        [SerializeField] private Button[] _characterButtons;

        public Action<int> OnCharacterSelectionChanged;

        private List<CharacterModel> _characterModels;

        public string PlayerInfo
        {
            set => _playerInfo.text = value;
        }

        private void OnEnable()
        {
            Reset();
        }

        private void Reset()
        {
            _characterModels = Storefront.Get().GetAllCharacterModels();
            Assert.IsTrue(_characterModels.Count == _characterButtons.Length, "characters.Count == _characterButtons.Length");
            _characterModels.Sort((a, b) => string.Compare(a.Name, b.Name, StringComparison.Ordinal));
            for (var i = 0; i < _characterModels.Count; ++i)
            {
                var character = _characterModels[i];
                var button = _characterButtons[i];
                button.SetCaption(character.Name);
                button.interactable = true;
                var capturedCharacterId = character.Id;
                button.onClick.AddListener(() => { OnCharacterSelectionChanged(capturedCharacterId); });
            }
        }

        public void ShowCharacter(int characterModelId)
        {
            for (var i = 0; i < _characterModels.Count; ++i)
            {
                var character = _characterModels[i];
                var button = _characterButtons[i];
                button.interactable = character.Id != characterModelId;
            }
        }
    }
}