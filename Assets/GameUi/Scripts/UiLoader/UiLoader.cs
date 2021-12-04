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
            var wait = new WaitForSeconds(_demoLoadDelay);
            yield return wait;
            var playerData = RuntimeGameConfig.Get().PlayerDataCache;
            if (playerData.IsValid)
            {
                WindowManager.Get().ShowWindow(_windowMainMenu);
                yield break;
            }
            WindowManager.Get().ShowWindow(_windowFirstTime);
        }
    }
}