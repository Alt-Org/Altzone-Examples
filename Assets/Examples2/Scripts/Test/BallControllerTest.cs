using System;
using System.Collections;
using Examples2.Scripts.Battle.Factory;
using Examples2.Scripts.Battle.interfaces;
using Photon.Pun;
using UnityConstants;
using UnityEngine;

namespace Examples2.Scripts.Test
{
    internal class BallControllerTest : MonoBehaviour
    {
        [Serializable]
        internal class State
        {
            public bool _isRedTeamActive;
            public bool _isBlueTeamActive;
        }

        [SerializeField] private State _state;

        [Header("Debug Only")] public Vector2 _ballVelocity;
        public bool _startBallMoving;
        public bool _stopBallMoving;
        public bool _hideBall;
        public bool _showBall;
        public bool _ghostBall;

        private IBall _ball;
        private IBrickManager _brickManager;

        private void Awake()
        {
            if (!PhotonNetwork.IsMasterClient)
            {
                enabled = false;
            }
            _ball = Context.GetBall;
            _brickManager = Context.GetBrickManager;
            var ballCollision = _ball.BallCollision;
            ballCollision.OnHeadCollision = OnHeadCollision;
            ballCollision.OnShieldCollision = OnShieldCollision;
            ballCollision.OnBrickCollision = OnBrickCollision;
            ballCollision.OnWallCollision = OnWallCollision;
            ballCollision.OnEnterTeamArea = OnEnterTeamArea;
            ballCollision.OnExitTeamArea = OnExitTeamArea;
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

        #region IBallCollision callback events

        private void OnHeadCollision(GameObject other)
        {
            Debug.Log($"onHeadCollision {other.name}");
        }

        private void OnShieldCollision(GameObject other)
        {
            Debug.Log($"onShieldCollision {other.name}");
        }

        private void OnBrickCollision(GameObject other)
        {
            //Debug.Log($"onBrickCollision {other.name}");
            _brickManager.DeleteBrick(other);
        }

        private void OnWallCollision(GameObject other)
        {
            Debug.Log($"onWallCollision {other.name} {other.tag}");
        }

        private void OnEnterTeamArea(GameObject other)
        {
            //Debug.Log($"onEnterTeamArea {other.name} {other.tag}");
            switch (other.tag)
            {
                case Tags.RedTeam:
                    _state._isRedTeamActive = true;
                    SetBallColor(_ball, _state);
                    return;
                case Tags.BlueTeam:
                    _state._isBlueTeamActive = true;
                    SetBallColor(_ball, _state);
                    return;
            }
            Debug.Log($"UNHANDLED onEnterTeamArea {other.name} {other.tag}");
        }

        private void OnExitTeamArea(GameObject other)
        {
            //Debug.Log($"onExitTeamArea {other.name} {other.tag}");
            switch (other.tag)
            {
                case Tags.RedTeam:
                    _state._isRedTeamActive = false;
                    SetBallColor(_ball, _state);
                    return;
                case Tags.BlueTeam:
                    _state._isBlueTeamActive = false;
                    SetBallColor(_ball, _state);
                    return;
            }
            Debug.Log($"UNHANDLED onExitTeamArea {other.name} {other.tag}");
        }

        #endregion

        private static void SetBallColor(IBall ball, State state)
        {
            if (state._isRedTeamActive && !state._isBlueTeamActive)
            {
                ball.SetColor(BallColor.RedTeam);
                return;
            }
            if (state._isBlueTeamActive && !state._isRedTeamActive)
            {
                ball.SetColor(BallColor.BlueTeam);
                return;
            }
            ball.SetColor(BallColor.NoTeam);
        }
    }
}