using Altzone.Scripts.ScriptableObjects;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.UI;

namespace Altzone.Scripts.Window
{
    [RequireComponent(typeof(Button))]
    public class NaviButton : MonoBehaviour
    {
        [SerializeField] private WindowDef _naviTarget;

        private void Awake()
        {
            Assert.IsNotNull(_naviTarget, "_naviTarget != null");
            var button = GetComponent<Button>();
            button.onClick.AddListener(() =>
            {
                Debug.Log($"Click {_naviTarget}");
                WindowManager.Get().ShowWindow(_naviTarget);
            });
        }
    }
}