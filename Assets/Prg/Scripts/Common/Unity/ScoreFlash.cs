using System;
using UnityEngine;
using TMPro;

namespace Prg.Scripts.Common.Unity
{
    public static class ScoreFlash
    {
        public static void Push(string message)
        {
            ScoreFlasher.Get().Push(message);
        }
    }

    public interface IScoreFlash
    {
        void Push(string message);
    }

    public class ScoreFlasher : MonoBehaviour, IScoreFlash
    {
        private static IScoreFlash _instance;

        public static IScoreFlash Get()
        {
            if (_instance == null)
            {
                _instance = FindObjectOfType<ScoreFlasher>();
                if (_instance == null)
                {
                    var instance = UnityExtensions.CreateGameObjectAndComponent<ScoreFlasher>(nameof(ScoreFlasher), true);
                    _instance = instance;
                    var config = Resources.Load<ScoreFlashConfig>(nameof(ScoreFlashConfig));
                    instance.Setup(config);
                }
            }
            return _instance;
        }

        [SerializeField] private Canvas _canvas;
        [SerializeField] private TMP_Text _text;

        private void Setup(ScoreFlashConfig config)
        {
            var canvas = Instantiate(config._canvasPrefab, Vector3.zero, Quaternion.identity);
            canvas.transform.SetParent(transform);
            _canvas = canvas;
            _text = canvas.GetComponentInChildren<TMP_Text>();
            _text.text = string.Empty;
        }

        void IScoreFlash.Push(string message)
        {
            Debug.Log($"Push {message}");
            _text.text = message;
        }
    }
}