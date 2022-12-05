using Altzone.Scripts.Config;
using Altzone.Scripts.Model;
using UnityEngine;

namespace GameUi.Scripts.ClanManagement
{
    public class LeaveClanController : MonoBehaviour
    {
        [SerializeField] private LeaveClanView _view;

        private void OnEnable()
        {
            _view.ResetView();
            var playerData = GameConfig.Get().PlayerDataCache;
            _view.PlayerInfo = playerData.PlayerName;
            var clanId = playerData.ClanId;
            var clan = Storefront.Get().GetClanModel(clanId);
            if (clan == null)
            {
                return;
            }
            _view.ClanInfo = $"Clan: {clan.Name}";
            var button = _view.LeaveClanButton;
            button.interactable = true;
            button.onClick.AddListener(LeaveClanButton);
        }

        private static void LeaveClanButton()
        {
            var playerData = GameConfig.Get().PlayerDataCache;
            playerData.UpdateClanId(-1);
        }
    }
}