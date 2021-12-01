using Altzone.Scripts.ScriptableObjects;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.UI;

namespace Altzone.Scripts.Window
{
    /// <summary>
    /// Default navigation button for <c>WindowManager</c>.
    /// </summary>
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