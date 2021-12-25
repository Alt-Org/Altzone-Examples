using Altzone.Scripts.Config;
using Altzone.Scripts.Model;
using Prg.Scripts.Common.Unity.Localization;
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
            _view.PlayerInfo = $"{playerData.PlayerName} - {playerData.CharacterModel.Name}";
            var clanId = playerData.ClanId;
            if (clanId > 0)
            {
                _view.Title = Localizer.Localize("JoinClan/TitleText2");
                var clan = Storefront.Get().GetClanModel(clanId);
                _view.ClanInfo = $"Clan: {clan.Name}";
                _view.AddButton("Leave Clan", LeaveClanButton);
                return;
            }
            _view.Title = Localizer.Localize("JoinClan/TitleText1");
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

        private void LeaveClanButton()
        {
            var playerData = RuntimeGameConfig.Get().PlayerDataCache;
            playerData.BatchSave(() =>
            {
                playerData.ClanId = -1;
            });
            LoadClanInfo();
        }
    }
}
