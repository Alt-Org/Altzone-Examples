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
                _animators[i] = new Animator();
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
            [SerializeField] private float _duration;
            [SerializeField] private float _expirationTime;
            [SerializeField] private MessageEntry _entry;

            public bool IsWorking => _duration < _expirationTime;

            internal void Animate(float elapsedTime)
            {
                _duration += elapsedTime;
                if (!IsWorking)
                {
                    _entry.Hide();
                }
            }

            public void Start(MessageEntry entry)
            {
                _duration = 0;
                _expirationTime = 2f;
                _entry = entry;
                entry.Show();
            }
        }
    }
}