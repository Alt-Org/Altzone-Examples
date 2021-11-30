using Altzone.Scripts.ScriptableObjects;
using UnityEngine.Assertions;
using UnityEngine.SceneManagement;

namespace Altzone.Scripts.Window
{
    public static class SceneLoader
    {
        public static void LoadScene(WindowDef windowDef)
        {
            Assert.IsFalse(string.IsNullOrEmpty(windowDef.SceneName), "string.IsNullOrEmpty(windowDef.SceneName)");
            SceneManager.LoadScene(windowDef.SceneName);
        }
    }
}