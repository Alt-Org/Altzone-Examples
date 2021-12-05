using System.Collections;
using Altzone.Scripts.Config;
using Altzone.Scripts.Window;
using Altzone.Scripts.Window.ScriptableObjects;
using UnityEngine;

namespace GameUi.Scripts.UiLoader
{
    public class UiLoader : MonoBehaviour
    {
        [SerializeField] private WindowDef _windowMainMenu;
        [SerializeField] private WindowDef _windowFirstTime;
        [SerializeField] private float _demoLoadDelay;

        private IEnumerator Start()
        {
            var playerData = RuntimeGameConfig.Get().PlayerDataCache;
            if (playerData.IsValid)
            {
                StartCoroutine(LoadNextWindow());
                yield break;
            }
            var wait = new WaitForSeconds(_demoLoadDelay);
            yield return wait;
            WindowManager.Get().ShowWindow(_windowFirstTime);
        }

        private IEnumerator LoadNextWindow()
        {
            yield return null;
            WindowManager.Get().ShowWindow(_windowMainMenu);
        }
    }
}