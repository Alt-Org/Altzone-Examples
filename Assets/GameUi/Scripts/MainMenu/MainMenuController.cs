using Altzone.Scripts.Config;
using UnityEngine;
using UnityEngine.Assertions;

namespace GameUi.Scripts.MainMenu
{
    public class MainMenuController : MonoBehaviour
    {
        [SerializeField] private MainMenuView _view;

        private void Awake()
        {
            Assert.IsNotNull(_view, "_view != null");
            var playerData = RuntimeGameConfig.GetPlayerDataCacheInEditor();
            Debug.Log(playerData.ToString());
        }
    }
}