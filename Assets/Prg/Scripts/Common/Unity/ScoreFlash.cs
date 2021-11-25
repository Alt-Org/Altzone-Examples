using System;
using System.Collections;
using UnityEngine;
using TMPro;
using UnityEngine.Assertions;

namespace Prg.Scripts.Common.Unity
{
    public static class ScoreFlash
    {
        public static void Push(string message)
        {
            ScoreFlasher.Get().Push(message, 0f, 0f);
        }

        public static void Push(string message, float x, float y)
        {
            ScoreFlasher.Get().Push(message, x, y);
        }

        public static void Push(string message, Vector2 position)
        {
            ScoreFlasher.Get().Push(message, position.x, position.y);
        }

        public static void Push(string message, Vector3 position)
        {
            ScoreFlasher.Get().Push(message, position.x, position.y);
        }
    }

    public interface IScoreFlash
    {
        void Push(string message, float x, float y);
    }

    [Serializable]
    internal class MessageEntry
    {
        [SerializeField] private Canvas _canvas;
        [SerializeField] private RectTransform _canvasRectTransform;
        [SerializeField] private GameObject _root;
        [SerializeField] private RectTransform _rectTransform;
        [SerializeField] private TMP_Text _text;
        [SerializeField] private Vector2 _initialPosition;
        [SerializeField] private Vector2 _position;
        [SerializeField] private Coroutine _routine;

        public Coroutine Routine => _routine;

        public MessageEntry(Canvas canvas, RectTransform canvasRectTransform, GameObject root, TMP_Text text)
        {
            _canvas = canvas;
            _canvasRectTransform = canvasRectTransform;
            _root = root;
            _rectTransform = _root.GetComponent<RectTransform>();
            _text = text;
            _initialPosition = _rectTransform.anchoredPosition;
            _text.text = string.Empty;
        }

        public void Move(float deltaX, float deltaY)
        {
        }

        public void SetScale(float scale)
        {
            var localScale = _rectTransform.localScale;
            localScale.x = scale;
            localScale.y = scale;
            _rectTransform.localScale = localScale;
        }

        public void SetColor(Color color)
        {
            _text.color = color;
        }

        public void SetText(string text, float x, float y, Coroutine routine)
        {
            _text.text = text;
            _position.x = x;
            _position.y = y;
            if (_routine != null)
            {
            }
            _routine = routine;
            if (_position == Vector2.zero)
            {
                _rectTransform.anchoredPosition = _initialPosition;
            }
            else
            {
                _rectTransform.anchoredPosition = _position;
            }
        }

        public void Show()
        {
            Assert.IsTrue(_root != null, "_root != null");
            _root.SetActive(true);
        }

        public void Hide()
        {
            _routine = null;
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

        [SerializeField] private Camera _camera;
        [SerializeField] private Canvas _canvas;
        [SerializeField] private MessageEntry[] _entries;
        [SerializeField] private Animator[] _animators;
        [SerializeField] private int _curIndex;

        private void Setup(ScoreFlashConfig config)
        {
            _camera = Camera.main;
            _canvas = Instantiate(config._canvasPrefab, Vector3.zero, Quaternion.identity);
            Assert.IsTrue(_canvas.isRootCanvas, "_canvas.isRootCanvas");
            DontDestroyOnLoad(_canvas);
            var canvasRectTransform = _canvas.GetComponent<RectTransform>();
            Debug.Log($"Setup canvas a-pos {canvasRectTransform.anchoredPosition} pixels {_canvas.pixelRect}");

            var children = _canvas.GetComponentsInChildren<TMP_Text>(true);
            Debug.Log($"Setup children {children.Length}");
            _entries = new MessageEntry[children.Length];
            _animators = new Animator[children.Length];
            for (int i = 0; i < children.Length; ++i)
            {
                var parent = children[i].transform.parent.gameObject;
                _entries[i] = new MessageEntry(_canvas, canvasRectTransform, parent, children[i]);
                _animators[i] = new Animator(config._phases);
            }
            _curIndex = -1;
            tempPosition.z = transform.position.z;
        }

        private void SetText(string text, float x, float y)
        {
            _curIndex += 1;
            if (_curIndex >= _entries.Length)
            {
                _curIndex = 0;
            }
            var routine = _entries[_curIndex].Routine;
            if (routine != null)
            {
                Debug.Log($"StopCoroutine {_curIndex}");
                StopCoroutine(_entries[_curIndex].Routine);
            }
            routine = StartCoroutine(AnimateText(_animators[_curIndex], _entries[_curIndex]));
            _entries[_curIndex].SetText(text, x, y, routine);
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

        private Vector3 tempPosition;
        private Vector3 screenPosition;

        void IScoreFlash.Push(string message, float x, float y)
        {
            /*tempPosition.x = x;
            tempPosition.y = y;
            screenPosition = _camera.WorldToScreenPoint(tempPosition);
            screenPosition.x -= 196f;
            screenPosition.y -= 348f;
            screenPosition.x *= 2f;
            screenPosition.y *= 2f;
            Debug.Log($"Push {message} @ x{x:F2} y{y:F2} -> x{screenPosition.x:F0} y{screenPosition.y:F0}");
            SetText(message, screenPosition.x, screenPosition.y);*/
            x *= 100f;
            y *= 100f;
            Debug.Log($"Push {message} @ x{x:F2} y{y:F2}");
            SetText(message, x, y);

        }

        [Serializable]
        private class Animator
        {
            private readonly ScoreFlashPhases _phases;

            [SerializeField] private float _duration;
            [SerializeField] private float _fraction;
            [SerializeField] private MessageEntry _entry;

            public bool IsWorking { get; private set; }

            public Animator(ScoreFlashPhases phases)
            {
                _phases = phases;
            }

            internal void Animate(float elapsedTime)
            {
                _duration += elapsedTime;
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
                if (_duration < _phases._fadeInTimeSeconds + _phases._readTimeSeconds + _phases._fadeOutTimeSeconds)
                {
                    FadeOutPhase();
                    return;
                }
                _entry.Hide();
                IsWorking = false;
            }

            public void Start(MessageEntry entry)
            {
                _duration = 0;
                IsWorking = true;
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
                var x = NGEasing.EaseOnCurve(_phases._fadeInOffsetXCurve, NGUtil.Scale(_phases._fadeInOffsetX), 0, _fraction);
                var y = NGEasing.EaseOnCurve(_phases._fadeInOffsetYCurve, NGUtil.Scale(_phases._fadeInOffsetY), 0, _fraction);
                _entry.Move(x, y);
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

                public static float EaseOnCurve(AnimationCurve curve, float from, float to, float time)
                {
                    float distance = to - from;
                    return from + curve.Evaluate(time) * distance;
                }
            }

            private static class NGUtil
            {
                public static float Scale(float pixels)
                {
                    // Remove later, used in ancient history era.
                    return pixels;
                }
            }
        }
    }
}