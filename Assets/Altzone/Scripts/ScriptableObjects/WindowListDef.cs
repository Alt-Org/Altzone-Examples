using UnityEngine;

namespace Altzone.Scripts.ScriptableObjects
{
    [CreateAssetMenu(menuName = "ALT-Zone/WindowListDef", fileName = "windows-")]
    public class WindowListDef : ScriptableObject
    {
        [SerializeField] private WindowDef[] _windowList;

        public WindowDef DefaultWindow => _windowList.Length > 0 ? _windowList[0] : null;
        public WindowDef[] Windows => _windowList;
    }
}