using System.Collections;
using Examples2.Scripts.Battle.Ball;
using Examples2.Scripts.Battle.Factory;
using Examples2.Scripts.Battle.interfaces;
using Photon.Pun;
using Prg.Scripts.Common.Unity;
using UnityEngine;

namespace Examples2.Scripts.Test
{
    internal class BallControllerTest : MonoBehaviour
    {
        [Header("Debug Only")] public Vector2 _ballVelocity;
        public bool _startBallMoving;
        public bool _stopBallMoving;
        public bool _hideBall;
        public bool _showBall;
        public bool _ghostBall;
        public bool _useScoreFlash;

        private IBall _ball;

        private void Awake()
        {
            if (!PhotonNetwork.IsMasterClient)
            {
                enabled = false;
            }
            _ball = Context.GetBall;
        }

        private IEnumerator Start()
        {
            if (_startBallMoving)
            {
                _startBallMoving = false;
                // Networking takes some time to establish ready state
                yield return new WaitForSeconds(1f);
                _startBallMoving = true;
            }
            _stopBallMoving = false;
            _hideBall = false;
            _showBall = false;
            _ghostBall = false;
        }

        private void Update()
        {
            if (_startBallMoving)
            {
                _startBallMoving = false;
                _ball.SetColor(BallColor.NoTeam);
                var position = GetComponent<Rigidbody2D>().position;
                _ball.StartMoving(position, _ballVelocity);
                return;
            }
            if (_stopBallMoving)
            {
                _stopBallMoving = false;
                _ball.StopMoving();
                _ball.SetColor(BallColor.Ghosted);
            }
            if (_hideBall)
            {
                _hideBall = false;
                _ball.StopMoving();
                _ball.SetColor(BallColor.Hidden);
                return;
            }
            if (_showBall)
            {
                _showBall = false;
                _ball.SetColor(BallColor.NoTeam);
                return;
            }
            if (_ghostBall)
            {
                _ghostBall = false;
                _ball.SetColor(BallColor.Ghosted);
            }
        }

        private void OnCollisionEnter2D(Collision2D other)
        {
            if (_useScoreFlash)
            {
                ScoreFlash.Push($"Hit {other.gameObject.name}");
            }
        }
    }
}