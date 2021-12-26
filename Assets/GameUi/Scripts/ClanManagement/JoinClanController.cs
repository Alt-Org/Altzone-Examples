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
            _view.PlayerInfo = playerData.PlayerName;
            var clanId = playerData.ClanId;
            var existingClan = Storefront.Get().GetClanModel(clanId);
            if (existingClan != null)
            {
                _view.ClanInfo = existingClan.Name;
                return;
            }
            _view.ClanInfo = Localizer.Localize("JoinClan/InfoText2");
            var clans = Storefront.Get().GetAllClanModels();
            foreach (var clan in clans)
            {
                var capturedClanId = clan.Id;
                var caption = $"{clan.Name} [{clan.Tag}]";
                _view.AddButton(caption, () =>
                {
                    Debug.Log($"SAVE CLAN {capturedClanId}");
                    playerData.BatchSave(() =>
                    {
                        playerData.ClanId = capturedClanId;
                    });
                });
            }
        }
    }
}
