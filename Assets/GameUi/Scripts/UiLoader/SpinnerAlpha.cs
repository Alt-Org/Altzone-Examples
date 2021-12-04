using UnityEngine;
using UnityEngine.UI;

namespace GameUi.Scripts.UiLoader
{
    public class SpinnerAlpha : MonoBehaviour
    {
        [SerializeField] private Image _image;
        [SerializeField, Min(0)] private float _alphaTime;

        private float _elapsedTime;
        private float _alphaRatio;
        private Color _currentColor;

        private void OnEnable()
        {
            _elapsedTime = 0;
            _currentColor = _image.color;
            _currentColor.a = 0;
            _image.color = _currentColor;
        }

        private void Update()
        {
            _elapsedTime += Time.deltaTime;
            if (_elapsedTime < _alphaTime)
            {
                _alphaRatio = _elapsedTime / _alphaTime;
                _currentColor.a = _alphaRatio;
                _image.color = _currentColor;
                return;
            }
            _currentColor.a = 1f;
            _image.color = _currentColor;
            enabled = false;
        }
    }
}
