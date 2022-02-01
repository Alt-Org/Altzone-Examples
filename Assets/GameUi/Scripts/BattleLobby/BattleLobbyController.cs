using System.Collections;
using Altzone.Scripts.Config;
using UnityEngine;

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
            StopAllCoroutines();
            StartCoroutine(WaitForRoom());
        }

        private IEnumerator WaitForRoom()
        {
            yield return null;
        }
    }
}