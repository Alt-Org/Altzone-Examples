using Examples2.Scripts.Battle.Factory;
using Examples2.Scripts.Battle.interfaces;
using Photon.Pun;
using System.Collections;
using UnityEngine;

namespace Examples2.Scripts.Battle.Ball
{
    internal class BallController : MonoBehaviour
    {
        public Vector2 ballVelocity;
        public Vector2 ballPosition;
        public bool startBallMoving;
        public bool stopBallMoving;

        private IBall ball;

        private void Awake()
        {
            if (!PhotonNetwork.IsMasterClient)
            {
                enabled = false;
            }
            ball = Context.getBall;
            var ballCollision = ball.ballCollision;
            ballCollision.onHeadCollision = onHeadCollision;
            ballCollision.onShieldCollision = onShieldCollision;
            ballCollision.onBrickCollision = onBrickCollision;
            ballCollision.onWallCollision = onWallCollision;
            ballCollision.onEnterTeamArea = onEnterTeamArea;
            ballCollision.onExitTeamArea = onExitTeamArea;
        }

        private IEnumerator Start()
        {
            ball.setColor(BallColor.Hidden);
            if (startBallMoving)
            {
                startBallMoving = false;
                yield return new WaitForSeconds(1f);
                startBallMoving = true;
            }
        }

        private void Update()
        {
            if (startBallMoving)
            {
                startBallMoving = false;
                ball.setColor(BallColor.NoTeam);
                ball.startMoving(ballPosition, ballVelocity);
                return;
            }
            if (stopBallMoving)
            {
                stopBallMoving = false;
                ball.stopMoving();
            }
        }

        #region IBallCollision callback events

        private void onHeadCollision(GameObject other)
        {
            Debug.Log($"onHeadCollision {other.name}");
        }

        private void onShieldCollision(GameObject other)
        {
            Debug.Log($"onShieldCollision {other.name}");
        }

        private void onBrickCollision(GameObject other)
        {
            Debug.Log($"onBrickCollision {other.name}");
        }

        private void onWallCollision(GameObject other)
        {
            Debug.Log($"onWallCollision {other.name} {other.tag}");
        }

        private void onEnterTeamArea(GameObject other)
        {
            Debug.Log($"onEnterTeamArea {other.tag}");
        }

        private void onExitTeamArea(GameObject other)
        {
            Debug.Log($"onExitTeamArea {other.tag}");
        }

        #endregion
    }
}