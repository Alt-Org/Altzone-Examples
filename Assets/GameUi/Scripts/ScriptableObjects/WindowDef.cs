using UnityEngine;

namespace GameUi.Scripts.ScriptableObjects
{
    [CreateAssetMenu(menuName = "ALT-Zone/WindowDef", fileName = "window-")]
    public class WindowDef : ScriptableObject
    {
        [SerializeField] private string _windowName;
        [SerializeField] private SceneDef _scene;

        public string WindowName => _windowName;
        public string SceneName => _scene != null ? _scene.SceneName : "";
    }
}