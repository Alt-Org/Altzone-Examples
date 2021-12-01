using System;
using Altzone.Scripts.ScriptableObjects;
using UnityEngine;

namespace Altzone.Scripts.Window
{
    public interface IWindowManager
    {
        void RegisterGoBackHandlerOnce(Func<WindowManager.GoBackAction> handler);
        void GoBack();
        void ShowWindow(WindowDef windowDef);
        void SetWindowsParent(GameObject windowsParent);
    }
}