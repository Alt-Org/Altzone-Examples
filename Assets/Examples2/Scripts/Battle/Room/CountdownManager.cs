using Examples2.Scripts.Battle.interfaces;
using TMPro;
using UnityEngine;

namespace Examples2.Scripts.Battle.Room
{
    /// <summary>
    /// Helper class to manage countdown counter.
    /// </summary>
    internal class CountdownManager : MonoBehaviour, ICountdownManager
    {
        [Header("Settings"), SerializeField] private TMP_Text _countdownText;

        private GameObject _countdown;

        private void Awake()
        {
            _countdown = _countdownText.gameObject;
            _countdown.SetActive(false);
        }

        void ICountdownManager.StartCountdown(int value)
        {
            _countdown.SetActive(true);
            _countdownText.text = value.ToString("N0");
        }

        void ICountdownManager.ShowCountdown(int value)
        {
            _countdownText.text = value.ToString("N0");
        }

        void ICountdownManager.HideCountdown()
        {
            _countdown.SetActive(false);
        }
    }
}