using UnityEngine;

namespace UiProto.Scripts.Window
{
    /// <summary>
    /// Handles ESCAPE key press using default WindowManager functionality.
    /// </summary>
    public class EscapeHandler : MonoBehaviour
    {
        private static EscapeHandler _instance;

        protected void Awake()
        {
            if (_instance == null)
            {
                // Register us as the singleton!
                _instance = this;
                var instance = gameObject.GetOrAddComponent<EscapeKeyPressed>();
                instance.AddListener(OnEscapeKeyPressed);
                return;
            }
            throw new UnityException($"Component added more than once: {nameof(EscapeHandler)}");
        }

        protected void OnDestroy()
        {
            if (_instance == this)
            {
                _instance = null;
            }
        }

        private static void OnEscapeKeyPressed()
        {
            WindowManager.SafeGoBack();
        }
    }
}