using Altzone.Scripts.Window.ScriptableObjects;
using Photon.Pun;
using UnityEngine.Assertions;
using UnityEngine.SceneManagement;

namespace Altzone.Scripts.Window
{
    /// <summary>
    /// Simple scene loader for <c>WindowManager</c>.
    /// </summary>
    internal static class SceneLoader
    {
        public static void LoadScene(WindowDef windowDef)
        {
            var scene = windowDef.Scene;
            Assert.IsNotNull(scene, "scene != null");
            Assert.IsFalse(string.IsNullOrEmpty(scene.SceneName), "string.IsNullOrEmpty(scene.SceneName)");
            if (scene.IsNetworkScene)
            {
                Debug.Log($"LoadScene NETWORK {scene.SceneName}");
                PhotonNetwork.LoadLevel(scene.SceneName);
            }
            else
            {
                Debug.Log($"LoadScene LOCAL {scene.SceneName}");
                SceneManager.LoadScene(scene.SceneName);
            }
        }
    }
}