using System;
using UnityEngine;

namespace Examples2.Scripts.Battle.Ball
{
    internal enum BallColor
    {
        NoTeam,
        RedTeam,
        BlueTeam,
        Ghosted,
        Hidden,
        Placeholder
    }

    [Serializable]
    internal class BallSettings
    {
        [Header("Ball Setup")] public GameObject ballCollider;
        public GameObject colorNoTeam;
        public GameObject colorRedTeam;
        public GameObject colorBlueTeam;
        public GameObject colorGhosted;
        public GameObject colorHidden;
        public GameObject colorPlaceholder;
    }

    [Serializable]
    internal class BallState
    {
        public BallColor ballColor;
        public bool isMoving;
    }

    internal class BallActor : MonoBehaviour
    {
        [SerializeField] private BallSettings settings;
        [SerializeField] private BallState state;

        private GameObject[] stateObjects;

        private void Awake()
        {
            stateObjects = new[]
            {
                settings.colorNoTeam,
                settings.colorRedTeam,
                settings.colorBlueTeam,
                settings.colorGhosted,
                settings.colorHidden,
                settings.colorPlaceholder
            };
        }

        private void OnEnable()
        {
        }

        private void stopMoving()
        {
            Debug.Log($"stopMoving {state.isMoving} <- {false}");
            state.isMoving = false;
            settings.ballCollider.SetActive(false);
        }

        private void startMoving()
        {
            Debug.Log($"startMoving {state.isMoving} <- {true}");
            state.isMoving = true;
            settings.ballCollider.SetActive(true);
        }

        private void setColor(BallColor ballColor)
        {
            Debug.Log($"setColor {state.ballColor} <- {ballColor}");
            stateObjects[(int)state.ballColor].SetActive(false);
            state.ballColor = ballColor;
            stateObjects[(int)state.ballColor].SetActive(true);
        }
    }
}