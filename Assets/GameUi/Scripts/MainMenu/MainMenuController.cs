using Altzone.Scripts.Config;
using UnityEngine;
using UnityEngine.Assertions;

namespace GameUi.Scripts.MainMenu
{
    public class MainMenuController : MonoBehaviour
    {
        [SerializeField] private MainMenuView _view;

        private void OnEnable()
        {
            Assert.IsNotNull(_view, "_view != null");
            var playerData = RuntimeGameConfig.Get().PlayerDataCache;
            Debug.Log(playerData.ToString());
        }
    }
}