using System;
using UnityEngine;

namespace Examples2.Scripts.Battle.Players
{
    internal abstract class PlayerActor : MonoBehaviour
    {
        internal const int PlayModeNormal = 0;
        internal const int PlayModeFrozen = 1;
        internal const int PlayModeGhosted = 2;

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