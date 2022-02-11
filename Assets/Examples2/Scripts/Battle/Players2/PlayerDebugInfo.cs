using System;
using Examples2.Scripts.Battle.Players;
using TMPro;
using UnityEngine;

namespace Examples2.Scripts.Battle.Players2
{
    internal class PlayerDebugInfo : MonoBehaviour
    {
        public TMP_Text _debugText;

        private string _myName;
        private string _currentText;

        public PlayerActor2 PlayerActor { get; set; }

        private void Awake()
        {
            _myName = transform.parent.name;
        }

        private void LateUpdate()
        {
            var text = $"{_myName} {PlayerActor.StateString}";
            if (text != _currentText)
            {
                _currentText = text;
                _debugText.text = text;
            }
        }
    }
}