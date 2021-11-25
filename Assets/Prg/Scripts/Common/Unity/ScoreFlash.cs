using System;
using System.Collections;
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

    [Serializable]
    internal class MessageEntry
    {
        [SerializeField] private GameObject _root;
        [SerializeField] private Transform _transform;
        [SerializeField] private TMP_Text _text;
        [SerializeField] private Vector3 _initialPosition;

        public MessageEntry(Transform transform, TMP_Text text)
        {
            _root = transform.gameObject;
            _transform = transform;
            _text = text;
            _initialPosition = transform.position;
            _text.text = string.Empty;
        }

        public void SetScale(float scale)
        {
            var localScale = _transform.localScale;
            localScale.x = scale;
            localScale.y = scale;
            _transform.localScale = localScale;
        }

        public void SetColor(Color color)
        {
            _text.color = color;
        }

        public void SetText(string text)
        {
            _text.text = text;
        }

        public void Show()
        {
            _root.SetActive(true);
            _transform.position = _initialPosition;
        }

        public void Hide()
        {
            _root.SetActive(false);
        }
    }

    internal class ScoreFlasher : MonoBehaviour, IScoreFlash
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
        [SerializeField] private MessageEntry[] _entries;
        [SerializeField] private Animator[] _animators;
        [SerializeField] private int _curIndex;

        private void Setup(ScoreFlashConfig config)
        {
            var canvas = Instantiate(config._canvasPrefab, Vector3.zero, Quaternion.identity);
            canvas.transform.SetParent(transform);
            _canvas = canvas;
            var children = canvas.GetComponentsInChildren<TMP_Text>(true);
            Debug.Log($"Setup children {children.Length}");
            _entries = new MessageEntry[children.Length];
            _animators = new Animator[children.Length];
            for (int i = 0; i < children.Length; ++i)
            {
                _entries[i] = new MessageEntry(children[i].transform.parent, children[i]);
                _animators[i] = new Animator(config._phases);
            }
            _curIndex = -1;
        }

        private void SetText(string text)
        {
            _curIndex += 1;
            if (_curIndex >= _entries.Length)
            {
                _curIndex = 0;
            }
            _entries[_curIndex].SetText(text);
            StartCoroutine(AnimateText(_animators[_curIndex], _entries[_curIndex]));
        }

        private static IEnumerator AnimateText(Animator animator, MessageEntry entry)
        {
            yield return null;
            animator.Start(entry);
            while (animator.IsWorking)
            {
                yield return null;
                animator.Animate(Time.deltaTime);
            }
        }

        void IScoreFlash.Push(string message)
        {
            Debug.Log($"Push {message}");
            SetText(message);
        }

        [Serializable]
        private class Animator
        {
            private readonly ScoreFlashPhases _phases;

            [SerializeField] private float _duration;
            [SerializeField] private float _expirationTime;
            [SerializeField] private float _fraction;
            [SerializeField] private MessageEntry _entry;

            public bool IsWorking => _duration < _expirationTime;

            public Animator(ScoreFlashPhases phases)
            {
                _phases = phases;
            }

            internal void Animate(float elapsedTime)
            {
                _duration += elapsedTime;
                if (!IsWorking)
                {
                    _entry.Hide();
                    return;
                }
                if (_duration < _phases._fadeInTimeSeconds)
                {
                    FadeInPhase();
                    return;
                }
                if (_duration < _phases._fadeInTimeSeconds + _phases._readTimeSeconds)
                {
                    StayVisiblePhase();
                    return;
                }
                FadeOutPhase();
            }

            public void Start(MessageEntry entry)
            {
                _duration = 0;
                _expirationTime = 2f;
                _entry = entry;
                entry.Show();
            }

            private void FadeInPhase()
            {
                _fraction = _duration / _phases._fadeInTimeSeconds;
                var textColor = NGEasing.EaseOnCurve(_phases._fadeInColorCurve, _phases._fadeInColor, _phases._readColorStart, _fraction);
                _entry.SetColor(textColor);
                var scale = NGEasing.EaseOnCurve(_phases._fadeInScaleCurve, _phases._fadeInScale, 1f, _fraction);
                _entry.SetScale(scale);
            }

            private void StayVisiblePhase()
            {
                _fraction = (_duration - _phases._fadeInTimeSeconds) / _phases._readTimeSeconds;
                var textColor = NGEasing.EaseOnCurve(_phases._readColorCurve, _phases._readColorStart, _phases._readColorEnd, _fraction);
                _entry.SetColor(textColor);
                var scale = NGEasing.EaseOnCurve(_phases._readScaleCurve, 1f, _phases._readScale, _fraction);
                _entry.SetScale(scale);
            }

            private void FadeOutPhase()
            {
                _fraction = (_duration - _phases._fadeInTimeSeconds - _phases._readTimeSeconds) / _phases._fadeOutTimeSeconds;
                var textColor = NGEasing.EaseOnCurve(_phases._fadeOutColorCurve, _phases._readColorEnd, _phases._fadeOutColor, _fraction);
                _entry.SetColor(textColor);
                var scale = NGEasing.EaseOnCurve(_phases._fadeOutScaleCurve, _phases._readScale, _phases._fadeOutScale, _fraction);
                _entry.SetScale(scale);
            }

            private static class NGEasing
            {
                public static Color EaseOnCurve(AnimationCurve curve, Color from, Color to, float time)
                {
                    Color distance = to - from;
                    return from + curve.Evaluate(time) * distance;
                }

                public static float EaseOnCurve(AnimationCurve curve, float from, float to, float time) {
                    float distance = to - from;
                    return from + curve.Evaluate(time) * distance;
                }
            }
        }
    }
}