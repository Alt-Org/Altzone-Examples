﻿using Prg.Scripts.Common.Unity;
using UnityEngine;

namespace Examples2.Scripts.Test
{
    internal class ScoreFlashTest : MonoBehaviour
    {
        public Rect _messageAreaScreen;
        public Camera _myCamera;
        public Vector2 _mousePosition;

        private void Awake()
        {
            var width = Screen.width / 2f;
            var height = Screen.height / 2f;
            _messageAreaScreen = Rect.MinMaxRect(-width, -height, width, height);
            Debug.Log($"Awake _messageAreaScreen {_messageAreaScreen}");
            Debug.Log($"Awake _messageAreaScreen x [{_messageAreaScreen.xMin} .. {_messageAreaScreen.xMax}] y [{_messageAreaScreen.yMin} .. {_messageAreaScreen.yMax}]");
            _myCamera = Camera.main;
            ScoreFlash.Push("Awake");
        }

        private void Update()
        {
            if (Input.GetMouseButtonUp(0))
            {
                _mousePosition = Input.mousePosition;
                var message = $"Click {_mousePosition}";
                Debug.Log(message);
                var worldPosition = _myCamera.ScreenToWorldPoint(_mousePosition);
                ScoreFlash.Push(message, worldPosition);
            }
        }
    }
}