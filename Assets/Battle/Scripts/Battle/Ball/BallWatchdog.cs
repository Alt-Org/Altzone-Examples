using Examples.Game.Scripts.Battle.interfaces;
using Examples.Game.Scripts.Battle.Scene;
using Photon.Pun;
using UnityEngine;

namespace Examples.Game.Scripts.Battle.Ball
{
    public class BallWatchdog : MonoBehaviour
    {
        [SerializeField] private Rect outerArea;
        [SerializeField] private Transform _transform;

        private void Awake()
        {
            if (!PhotonNetwork.IsMasterClient)
            {
                enabled = false;
                return;
            }
            var gameArena = SceneConfig.Get().gameArena;
            outerArea = gameArena.outerArea;
            _transform = transform;
        }

        private void Update()
        {
            if (outerArea.Contains(_transform.position))
            {
                return;
            }
            var ballControl = GetComponent<BallActor>() as IBallControl;
            ballControl.teleportBall(Vector2.zero, -1);
            global::Debug.LogWarning($"this is outside of the game arena: {name} pos {_transform.position}");
        }
    }
}