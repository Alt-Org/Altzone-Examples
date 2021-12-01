using UnityEngine;
using UnityEngine.UI;

namespace Altzone.Scripts.Window
{
    [RequireComponent(typeof(Button))]
    public class BackButton : MonoBehaviour
    {
        private void Awake()
        {
            var button = GetComponent<Button>();
            button.onClick.AddListener(() =>
            {
                WindowManager.Get().GoBack();
            });
        }
    }
}