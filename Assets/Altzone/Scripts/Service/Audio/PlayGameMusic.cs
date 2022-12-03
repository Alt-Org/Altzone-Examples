using UnityEngine;
using UnityEngine.UI;

namespace Altzone.Scripts.Service.Audio
{
    public class PlayGameMusic : MonoBehaviour
    {
        [SerializeField] private bool _register;
        [SerializeField] private AudioClip _audioClip;
        [SerializeField] private Button _button;

        private string _audioClipName;

        private void Awake()
        {
            if (_audioClip == null)
            {
                return;
            }
            var audioManager = AudioManager.Get();
            if (_register)
            {
                _audioClipName = $"music.{_audioClip.name}";
                audioManager.RegisterAudioClip(_audioClipName, _audioClip);
            }
            if (_button != null)
            {
                _button.onClick.AddListener(Play);
            }
        }

        public void Play()
        {
            var audioManager = AudioManager.Get();
            if (_register)
            {
                audioManager.PlayGameMusic(_audioClipName);
            }
            else
            {
                audioManager.PlayGameMusic(_audioClip);
            }
        }
    }
}