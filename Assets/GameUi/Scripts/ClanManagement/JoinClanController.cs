using Altzone.Scripts.Config;
using Altzone.Scripts.Model;
using UnityEngine;

namespace GameUi.Scripts.ClanManagement
{
    public class JoinClanController : MonoBehaviour
    {
        [SerializeField] private JoinClanView _view;

        private void OnEnable()
        {
            LoadClanInfo();
        }

        private void LoadClanInfo()
        {
            _view.ResetView();
            var playerData = RuntimeGameConfig.Get().PlayerDataCache;
            _view.PlayerInfo = playerData.PlayerName;
            var clanId = playerData.ClanId;
            var existingClan = Storefront.Get().GetClanModel(clanId);
            if (existingClan != null)
            {
                _view.ClanInfo = existingClan.Name;
                return;
            }
            _view.ClanInfo = $"Join Clan";
            var clans = Storefront.Get().GetAllClanModels();
            foreach (var clan in clans)
            {
                var capturedClanId = clan.Id;
                _view.AddButton(clan.Name, () =>
                {
                    playerData.BatchSave(() =>
                    {
                        playerData.ClanId = capturedClanId;
                    });
                    LoadClanInfo();
                });
            }
        }
    }
}
