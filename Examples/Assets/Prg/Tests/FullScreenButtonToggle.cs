using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Prg.Tests
{
    /// <summary>
    /// Test script to toggle <c>Screen.fullScreen</c> state (which controls Android/IOS navigation bar and status bar).<br />
    /// https://zehfernando.com/2015/unity-tidbits-changing-the-visibility-of-androids-navigation-and-status-bars-and-implementing-immersive-mode/
    /// </summary>
    /// <remarks>
    /// Requires simulator or mobile device to work.
    /// </remarks>
    [RequireComponent(typeof(Button))]
    public class FullScreenButtonToggle : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI _resolutionLabel;
        [SerializeField] private TextMeshProUGUI _frameRateLabel;

        private void Awake()
        {
            var button = GetComponent<Button>();
            button.onClick.AddListener(() =>
            {
                Screen.fullScreen = !Screen.fullScreen;
                StartCoroutine(UpdateCaption(button));
            });
            StartCoroutine(UpdateCaption(button));
            if (_resolutionLabel != null)
            {
                ShowResolution(_resolutionLabel);
            }
            if (_frameRateLabel != null)
            {
                ShowFrameRate(_frameRateLabel);
            }
        }

        private static void ShowResolution(TextMeshProUGUI resolutionLabel)
        {
            var screenWidth = Screen.width;
            var screenHeight = Screen.height;
            var safeArea = Screen.safeArea;
            var isMobile = AppPlatform.IsMobile || AppPlatform.IsSimulator;
            var isFullSafeArea = !isMobile ||
                                 (Mathf.Approximately(safeArea.width, screenWidth) && Mathf.Approximately(safeArea.height, screenHeight));
            if (isFullSafeArea)
            {
                resolutionLabel.text = $"{screenWidth}x{screenHeight}";
                return;
            }
            resolutionLabel.text = $"{screenWidth}x{screenHeight}\n({safeArea.width}x{safeArea.height})";
        }

        private static void ShowFrameRate(TextMeshProUGUI frameRateLabel)
        {
            frameRateLabel.text = $"{Application.targetFrameRate} fps | {QualitySettings.vSyncCount} vsync";
        }
        
        private static IEnumerator UpdateCaption(Button button)
        {
            yield return null;
            var text = Screen.fullScreen ? "FULLSCREEN" : "WINDOW";
            button.SetCaption(text);
        }
    }
}