using System;
using UnityEngine;
using UnityEngine.UI;

namespace GameUi.Scripts.ClanManagement
{
    public class JoinClanView : MonoBehaviour
    {
        [SerializeField] private Text _playerInfo;
        [SerializeField] private Text _clanInfo;
        [SerializeField] private Transform _contentRoot;
        [SerializeField] private GameObject _buttonPrefab;

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
            var childCount = _contentRoot.childCount;
            for (var i = childCount - 1; i >= 0; --i)
            {
                Destroy(_contentRoot.GetChild(i).gameObject);
            }
        }

        public void AddButton(string buttonCaption, Action clickHandler)
        {
            var instance = Instantiate(_buttonPrefab, _contentRoot);
            var button = instance.GetComponentInChildren<Button>();
            button.SetCaption(buttonCaption);
            button.gameObject.SetActive(true);
            button.onClick.AddListener(() => clickHandler());
        }
    }
}