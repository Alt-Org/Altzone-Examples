using Altzone.Scripts.ScriptableObjects;
using UnityEngine;

namespace Altzone.Scripts.Window
{
    public class WindowManager : MonoBehaviour
    {
        public static void LoadWindow(WindowDef windowDef)
        {
            Debug.Log($"LoadWindow '{windowDef.WindowName}'");
        }
    }
}