using System;
using System.Collections;
using UnityEngine;
using TMPro;
using UnityEngine.Assertions;

namespace Prg.Scripts.Common.Unity
{
    /// <summary>
    /// Flash message system
    /// </summary>
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
        [SerializeField] private GameObject _root;
        [SerializeField] private RectTransform _rectTransform;

        [SerializeField] private TMP_Text _text;
        [SerializeField] private Vector3 _position;
        private Coroutine _routine;

        public Coroutine Routine => _routine;

        public MessageEntry(GameObject root, TMP_Text text)
        {
            _root = root;
            _rectTransform = _root.GetComponent<RectTransform>();
            _text = text;
            _position.z = _root.GetComponent<Transform>().position.z;
        }

        public void SetCoroutine(Coroutine routine)
        {
            _routine = routine;
        }

        public void SetText(string text)
        {
            _text.text = text;
        }

        public void SetPosition(float x, float y)
        {
            _position.x = x;
            _position.y = y;
            _rectTransform.anchoredPosition = _position;
        }

        public void Move(float deltaX, float deltaY)
        {
            SetPosition(_position.x + deltaX, _position.y + deltaY);
        }

        public void SetRotation(float angleZ)
        {
            _rectTransform.rotation = Quaternion.identity;
            _rectTransform.Rotate(0f, 0f, angleZ);
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
                    var instance = UnityExtensions.CreateGameObjectAndComponent<ScoreFlasher>(nameof(ScoreFlasher), false);
                    _instance = instance;
                    var config = Resources.Load<ScoreFlashConfig>(nameof(ScoreFlashConfig));
                    instance.Setup(config, Camera.main);
                }
            }
            return _instance;
        }

        [SerializeField] private Camera _camera;
        [SerializeField] private Canvas _canvas;
        [SerializeField] private RectTransform _canvasRectTransform;
        [SerializeField] private MessageEntry[] _entries;
        [SerializeField] private Animator[] _animators;
        [SerializeField] private int _curIndex;

        private Vector3 _worldPos;
        private Vector3 _screenPos;

        private void Setup(ScoreFlashConfig config, Camera screenCamera)
        {
            _camera = screenCamera;
            _canvas = Instantiate(config._canvasPrefab, Vector3.zero, Quaternion.identity);
            Assert.IsTrue(_canvas.isRootCanvas, "_canvas.isRootCanvas");
            _canvasRectTransform = _canvas.GetComponent<RectTransform>();

            var children = _canvas.GetComponentsInChildren<TMP_Text>(true);
            Debug.Log($"Setup children {children.Length}");
            _entries = new MessageEntry[children.Length];
            _animators = new Animator[children.Length];
            for (int i = 0; i < children.Length; ++i)
            {
                var parent = children[i].gameObject;
                _entries[i] = new MessageEntry(parent, children[i]);
                _entries[i].Hide();
                _animators[i] = new Animator(config._phases);
            }
            _curIndex = -1;
        }

        private void OnDestroy()
        {
            Debug.Log($"OnDestroy");
            _instance = null;
            for (var i = 0; i < _entries.Length; ++i)
            {
                StopCoroutine(i);
            }
        }

        private void SetText(string text, float x, float y)
        {
            _curIndex += 1;
            if (_curIndex >= _entries.Length)
            {
                _curIndex = 0;
            }
            StopCoroutine(_curIndex);
            var routine = StartCoroutine(AnimateText(_animators[_curIndex], _entries[_curIndex], text, x, y));
            _entries[_curIndex].SetCoroutine(routine);
        }

        private void StopCoroutine(int index)
        {
            var routine = _entries[index].Routine;
            if (routine != null)
            {
                Debug.Log($"StopCoroutine {index}");
                StopCoroutine(routine);
            }
        }

        private static IEnumerator AnimateText(Animator animator, MessageEntry entry, string text, float x, float y)
        {
            yield return null;
            animator.Start(entry, x, y);
            yield return null;
            entry.SetText(text);
            while (animator.IsWorking)
            {
                yield return null;
                animator.Animate(Time.deltaTime);
            }
        }

        void IScoreFlash.Push(string message, float worldX, float worldY)
        {
            _worldPos.x = worldX;
            _worldPos.y = worldY;
            _screenPos = _camera.WorldToScreenPoint(_worldPos);
            var hit = RectTransformUtility.ScreenPointToLocalPointInRectangle(
                _canvasRectTransform, _screenPos, null, out var localPos);
            Assert.IsTrue(hit, "RectTransformUtility.ScreenPointToLocalPointInRectangle was hit");
            Debug.Log($"Push {message} @ WorldToScreenPoint {(Vector2)_worldPos} -> {(Vector2)_screenPos} -> rect {localPos}");

            SetText(message, localPos.x, localPos.y);
        }

        [Serializable]
        private class Animator
        {
            private readonly ScoreFlashPhases _phases;

            [SerializeField] private float _duration;
            [SerializeField] private float _fraction;
            [SerializeField] private MessageEntry _entry;

            private float _fadeOutRotationAngle;
            private float _fadeOutRotationSpeed;

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
                    FadeInPhase(elapsedTime);
                    return;
                }
                if (_duration < _phases._fadeInTimeSeconds + _phases._readTimeSeconds)
                {
                    StayVisiblePhase(elapsedTime);
                    return;
                }
                if (_duration < _phases._fadeInTimeSeconds + _phases._readTimeSeconds + _phases._fadeOutTimeSeconds)
                {
                    FadeOutPhase(elapsedTime);
                    return;
                }
                _entry.Hide();
                IsWorking = false;
            }

            public void Start(MessageEntry entry, float x, float y)
            {
                _duration = 0;
                _fadeOutRotationAngle = 0;
                _fadeOutRotationSpeed = _phases._fadeOutInitialRotationSpeed;
                IsWorking = true;
                _entry = entry;
                _entry.SetColor(_phases._fadeInColor);
                _entry.SetScale(_phases._fadeInScale);
                _entry.SetText(string.Empty);
                _entry.SetPosition(x, y);
                _entry.SetRotation(0);
                _entry.Show();
            }

            private void FadeInPhase(float elapsedTime)
            {
                _fraction = _duration / _phases._fadeInTimeSeconds;
                var textColor = NgEasing.EaseOnCurve(_phases._fadeInColorCurve, _phases._fadeInColor, _phases._readColorStart, _fraction);
                _entry.SetColor(textColor);
                var scale = NgEasing.EaseOnCurve(_phases._fadeInScaleCurve, _phases._fadeInScale, 1f, _fraction);
                _entry.SetScale(scale);
                var x = NgEasing.EaseOnCurve(_phases._fadeInOffsetXCurve, _phases._fadeInOffsetX, 0, _fraction);
                var y = NgEasing.EaseOnCurve(_phases._fadeInOffsetYCurve, _phases._fadeInOffsetY, 0, _fraction);
                _entry.Move(x * elapsedTime, y * elapsedTime);
            }

            private void StayVisiblePhase(float elapsedTime)
            {
                _fraction = (_duration - _phases._fadeInTimeSeconds) / _phases._readTimeSeconds;
                var textColor = NgEasing.EaseOnCurve(_phases._readColorCurve, _phases._readColorStart, _phases._readColorEnd, _fraction);
                _entry.SetColor(textColor);
                var scale = NgEasing.EaseOnCurve(_phases._readScaleCurve, 1f, _phases._readScale, _fraction);
                _entry.SetScale(scale);
                var x = NgEasing.EaseOnCurve(_phases._readVelocityXCurve, 0, _phases._readFloatRightVelocity, _fraction);
                var y = NgEasing.EaseOnCurve(_phases._readVelocityCurve, 0, _phases._readFloatUpVelocity, _fraction);
                _entry.Move(x * elapsedTime, -y * elapsedTime);
            }

            private void FadeOutPhase(float elapsedTime)
            {
                _fraction = (_duration - _phases._fadeInTimeSeconds - _phases._readTimeSeconds) / _phases._fadeOutTimeSeconds;
                var textColor = NgEasing.EaseOnCurve(_phases._fadeOutColorCurve, _phases._readColorEnd, _phases._fadeOutColor, _fraction);
                _entry.SetColor(textColor);
                var scale = NgEasing.EaseOnCurve(_phases._fadeOutScaleCurve, _phases._readScale, _phases._fadeOutScale, _fraction);
                _entry.SetScale(scale);
                var x = NgEasing.EaseOnCurve(
                    _phases._fadeOutVelocityXCurve, _phases._readFloatRightVelocity, _phases._fadeOutFloatRightVelocity, _fraction);
                var y = NgEasing.EaseOnCurve(
                    _phases._fadeOutVelocityCurve, _phases._readFloatUpVelocity, _phases._fadeOutFloatUpVelocity, _fraction);
                _entry.Move(x * elapsedTime, -y * elapsedTime);
                _fadeOutRotationSpeed += _phases._fadeOutRotationAcceleration * elapsedTime;
                _fadeOutRotationAngle += _fadeOutRotationSpeed * elapsedTime;
                _entry.SetRotation(_fadeOutRotationAngle);
            }

            private static class NgEasing
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
        }
    }
}