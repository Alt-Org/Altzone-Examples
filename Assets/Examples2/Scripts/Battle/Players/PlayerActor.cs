using System;
using UnityEngine;

namespace Examples2.Scripts.Battle.Players
{
    internal abstract class PlayerActor : MonoBehaviour
    {
        public const int PlayModeNormal = 0;
        public const int PlayModeFrozen = 1;
        public const int PlayModeGhosted = 2;

        [Serializable]
        internal class PlayerState
        {
            public int _currentMode;
            public Transform _transform;
            public int _playerPos;
            public int _teamNumber;
            public PlayerActor _teamMate;
        }

        [SerializeField] protected PlayerState _state;

        public int PlayerPos => _state._playerPos;
        public int TeamNumber => _state._teamNumber;
    }
}