using UnityEngine;

namespace Altzone.Scripts.ScriptableObjects
{
    [CreateAssetMenu(menuName = "ALT-Zone/WindowDef", fileName = "window-")]
    public class WindowDef : ScriptableObject
    {
        [SerializeField] private GameObject _windowPrefab;
        [SerializeField] private SceneDef _scene;

        public bool HasScene => _scene != null;
        public GameObject WindowPrefab => _windowPrefab;
        public string WindowName => _windowPrefab != null ? _windowPrefab.name : string.Empty;
        public string SceneName => _scene != null ? _scene.SceneName : string.Empty;
        public SceneDef Scene => _scene;

        public override string ToString()
        {
            return HasScene ? $"WindowDef: '{WindowName}' ({SceneName})" : $"WindowDef: '{WindowName}'";
        }
    }
}