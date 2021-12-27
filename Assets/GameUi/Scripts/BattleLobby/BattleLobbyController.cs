using Altzone.Scripts.Config;
using UnityEngine;
using UnityEngine.Assertions;

namespace GameUi.Scripts.BattleLobby
{
    public class BattleLobbyController : MonoBehaviour
    {
        [SerializeField] private BattleLobbyView _view;

        private void OnEnable()
        {
            var playerData = RuntimeGameConfig.Get().PlayerDataCache;
            Debug.Log($"OnEnable {playerData}");
            _view.ResetView();
            _view.PlayerInfo = playerData.GetPlayerInfoLabel();
        }
    }
}