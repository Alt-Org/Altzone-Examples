using UnityEngine;
using UnityEngine.SceneManagement;

namespace Altzone.Scripts.Window.ScriptableObjects
{
    /// <summary>
    /// Window definition for <c>WindowManager</c>.<br />
    /// It consists of window prefab (what to show) and optional scene  definition (where to show).
    /// </summary>
    [CreateAssetMenu(menuName = "ALT-Zone/WindowDef", fileName = "window-")]
    public class WindowDef : ScriptableObject
    {
        [SerializeField] private GameObject _windowPrefab;
        [SerializeField] private SceneDef _scene;

        public bool HasScene => _scene != null;
        public bool NeedsSceneLoad => _NeedsSceneLoad();
        public GameObject WindowPrefab => _windowPrefab;
        public string WindowName => _windowPrefab != null ? _windowPrefab.name : string.Empty;
        public string SceneName => _scene != null ? _scene.SceneName : string.Empty;
        public SceneDef Scene => _scene;

        private bool _NeedsSceneLoad()
        {
            if (_scene == null)
            {
                return false;
            }
            var currentSceneName = SceneManager.GetActiveScene().name;
            Debug.Log($"_NeedsSceneLoad {currentSceneName} <-> {_scene} : {currentSceneName != _scene.SceneName}");
            return currentSceneName != _scene.SceneName;
        }

        public override string ToString()
        {
            return HasScene ? $"WindowDef: '{WindowName}' ({SceneName})" : $"WindowDef: '{WindowName}'";
        }
    }
}