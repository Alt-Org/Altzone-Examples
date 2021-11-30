using UnityEngine;

namespace UiProto.Scripts.Window
{
    /// <summary>
    /// Handles ESCAPE key press using default WindowManager functionality.
    /// </summary>
    public class EscapeHandler : MonoBehaviour
    {
        protected void Awake()
        {
            var instance = gameObject.GetOrAddComponent<EscapeKeyPressed>();
            instance.AddListener(OnEscapeKeyPressed);
        }

        private static void OnEscapeKeyPressed()
        {
            WindowManager.SafeGoBack();
        }
    }
}