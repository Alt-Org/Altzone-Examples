using Altzone.Scripts.Config;
using UnityEngine;
using UnityEngine.Assertions;

namespace GameUi.Scripts.BattleLobby
{
    public class BattleLobbyController : MonoBehaviour
    {
        [SerializeField] private BattleLobbyView _view;

        private void Awake()
        {
            Assert.IsNotNull(_view, "_view != null");
            var playerData = RuntimeGameConfig.Get().PlayerDataCache;
            Debug.Log(playerData.ToString());
            _view.PlayerInfo = $"{playerData.PlayerName} : {playerData.CharacterModel.Name}";
            _view.StartGameButton.interactable = false;
        }
    }
}