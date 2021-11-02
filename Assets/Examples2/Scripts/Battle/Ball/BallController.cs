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
    }
}