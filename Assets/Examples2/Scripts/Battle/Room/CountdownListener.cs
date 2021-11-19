using Prg.Scripts.Common.PubSub;
using TMPro;
using UnityEngine;

namespace Examples2.Scripts.Battle.Room
{
    /// <summary>
    /// Helper class to manage countdown counter.
    /// </summary>
    internal class CountdownListener : MonoBehaviour
    {
        [Header("Settings"), SerializeField] private TextMeshPro _countdownText;

        private GameObject _countdown;

        private void Awake()
        {
            _countdown = _countdownText.gameObject;
            _countdown.SetActive(false);
            this.Subscribe<PlayerManager.CountdownEvent>(OnCountdownEvent);
        }

        private void OnCountdownEvent(PlayerManager.CountdownEvent data)
        {
            if (data.CurValue == data.MaxValue)
            {
                StartCountdown(data.CurValue);
                return;
            }
            if (data.CurValue >= 0)
            {
                ShowCountdown(data.CurValue);
                return;
            }
            HideCountdown();
            this.Unsubscribe();
        }

        private void StartCountdown(int value)
        {
            _countdown.SetActive(true);
            _countdownText.text = value.ToString("N0");
        }

        private void ShowCountdown(int value)
        {
            _countdownText.text = value.ToString("N0");
        }

        private void HideCountdown()
        {
            _countdown.SetActive(false);
        }
    }
}