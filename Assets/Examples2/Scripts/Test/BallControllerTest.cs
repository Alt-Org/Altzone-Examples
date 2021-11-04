using Examples2.Scripts.Battle.Factory;
using Examples2.Scripts.Battle.interfaces;
using Photon.Pun;
using System;
using System.Collections;
using UnityEngine;

namespace Examples2.Scripts.Test
{
    internal class BallControllerTest : MonoBehaviour
    {
        [Serializable]
        internal class State
        {
            public bool isRedTeamActive;
            public bool isBlueTeamActive;
        }

        [SerializeField] private State state;

        [Header("Debug Only")] public Vector2 ballVelocity;
        public bool startBallMoving;
        public bool stopBallMoving;
        public bool hideBall;
        public bool showBall;
        public bool ghostBall;

        private IBall _ball;
        private IBrickManager _brickManager;

        private void Awake()
        {
            if (!PhotonNetwork.IsMasterClient)
            {
                enabled = false;
            }
            _ball = Context.GETBall;
            _brickManager = Context.GETBrickManager;
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
            _ball.SetColor(BallColor.Hidden);
            if (startBallMoving)
            {
                startBallMoving = false;
                yield return new WaitForSeconds(1f);
                startBallMoving = true;
            }
            stopBallMoving = false;
            hideBall = false;
            showBall = false;
            ghostBall = false;
        }

        private void Update()
        {
            if (startBallMoving)
            {
                startBallMoving = false;
                _ball.SetColor(BallColor.NoTeam);
                var position = GetComponent<Rigidbody2D>().position;
                _ball.StartMoving(position, ballVelocity);
                return;
            }
            if (stopBallMoving)
            {
                stopBallMoving = false;
                _ball.StopMoving();
                _ball.SetColor(BallColor.Ghosted);
            }
            if (hideBall)
            {
                hideBall = false;
                _ball.StopMoving();
                _ball.SetColor(BallColor.Hidden);
                return;
            }
            if (showBall)
            {
                showBall = false;
                _ball.SetColor(BallColor.NoTeam);
                return;
            }
            if (ghostBall)
            {
                ghostBall = false;
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
                case UnityConstants.Tags.RedTeam:
                    state.isRedTeamActive = true;
                    SetBallColor(_ball, state);
                    return;
                case UnityConstants.Tags.BlueTeam:
                    state.isBlueTeamActive = true;
                    SetBallColor(_ball, state);
                    return;
            }
            Debug.Log($"UNHANDLED onEnterTeamArea {other.name} {other.tag}");
        }

        private void OnExitTeamArea(GameObject other)
        {
            //Debug.Log($"onExitTeamArea {other.name} {other.tag}");
            switch (other.tag)
            {
                case UnityConstants.Tags.RedTeam:
                    state.isRedTeamActive = false;
                    SetBallColor(_ball, state);
                    return;
                case UnityConstants.Tags.BlueTeam:
                    state.isBlueTeamActive = false;
                    SetBallColor(_ball, state);
                    return;
            }
            Debug.Log($"UNHANDLED onExitTeamArea {other.name} {other.tag}");
        }

        #endregion

        private static void SetBallColor(IBall ball, State state)
        {
            if (state.isRedTeamActive && !state.isBlueTeamActive)
            {
                ball.SetColor(BallColor.RedTeam);
                return;
            }
            if (state.isBlueTeamActive && !state.isRedTeamActive)
            {
                ball.SetColor(BallColor.BlueTeam);
                return;
            }
            ball.SetColor(BallColor.NoTeam);
        }
    }
}