using System;
using UnityEngine;
using UnityEngine.UI;

namespace GameUi.Scripts.ClanManagement
{
    public class JoinClanView : MonoBehaviour
    {
        [SerializeField] private Text _playerInfo;
        [SerializeField] private Text _clanInfo;
        [SerializeField] private Button[] _buttons;

        public string PlayerInfo
        {
            set => _playerInfo.text = value;
        }

        public string ClanInfo
        {
            set => _clanInfo.text = value;
        }

        public void ResetView()
        {
            PlayerInfo = string.Empty;
            ClanInfo = string.Empty;
            foreach (var button in _buttons)
            {
                button.gameObject.SetActive(false);
                button.onClick.RemoveAllListeners();
            }
        }

        public void AddButton(string buttonCaption, Action clickHandler)
        {
            var button = GetFreeButton();
            button.SetCaption(buttonCaption);
            button.gameObject.SetActive(true);
            button.onClick.AddListener(() => clickHandler());
        }

        private Button GetFreeButton()
        {
            foreach (var button in _buttons)
            {
                if (!button.gameObject.activeSelf)
                {
                    return button;
                }
            }
            throw new UnityException("Run out of buttons");
        }
    }
}