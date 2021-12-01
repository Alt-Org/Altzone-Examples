using System;
using Altzone.Scripts.ScriptableObjects;
using UnityEngine;

namespace Altzone.Scripts.Window
{
    /// <summary>
    /// Interface for simple <c>WindowManager</c>.
    /// </summary>
    public interface IWindowManager
    {
        void RegisterGoBackHandlerOnce(Func<WindowManager.GoBackAction> handler);
        void GoBack();
        void ShowWindow(WindowDef windowDef);
        void SetWindowsParent(GameObject windowsParent);
    }
}