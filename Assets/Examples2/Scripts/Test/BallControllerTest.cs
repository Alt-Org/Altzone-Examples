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

        private IBall ball;
        private IBrickManager brickManager;

        private void Awake()
        {
            if (!PhotonNetwork.IsMasterClient)
            {
                enabled = false;
            }
            ball = Context.getBall;
            brickManager = Context.getBrickManager;
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
                ball.setColor(BallColor.NoTeam);
                var position = GetComponent<Rigidbody2D>().position;
                ball.startMoving(position, ballVelocity);
                return;
            }
            if (stopBallMoving)
            {
                stopBallMoving = false;
                ball.stopMoving();
                ball.setColor(BallColor.Ghosted);
            }
            if (hideBall)
            {
                hideBall = false;
                ball.stopMoving();
                ball.setColor(BallColor.Hidden);
                return;
            }
            if (showBall)
            {
                showBall = false;
                ball.setColor(BallColor.NoTeam);
                return;
            }
            if (ghostBall)
            {
                ghostBall = false;
                ball.setColor(BallColor.Ghosted);
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
            //Debug.Log($"onBrickCollision {other.name}");
            brickManager.deleteBrick(other);
        }

        private void onWallCollision(GameObject other)
        {
            Debug.Log($"onWallCollision {other.name} {other.tag}");
        }

        private void onEnterTeamArea(GameObject other)
        {
            //Debug.Log($"onEnterTeamArea {other.name} {other.tag}");
            switch (other.tag)
            {
                case UnityConstants.Tags.RedTeam:
                    state.isRedTeamActive = true;
                    setBallColor(ball, state);
                    return;
                case UnityConstants.Tags.BlueTeam:
                    state.isBlueTeamActive = true;
                    setBallColor(ball, state);
                    return;
            }
            Debug.Log($"UNHANDLED onEnterTeamArea {other.name} {other.tag}");
        }

        private void onExitTeamArea(GameObject other)
        {
            //Debug.Log($"onExitTeamArea {other.name} {other.tag}");
            switch (other.tag)
            {
                case UnityConstants.Tags.RedTeam:
                    state.isRedTeamActive = false;
                    setBallColor(ball, state);
                    return;
                case UnityConstants.Tags.BlueTeam:
                    state.isBlueTeamActive = false;
                    setBallColor(ball, state);
                    return;
            }
            Debug.Log($"UNHANDLED onExitTeamArea {other.name} {other.tag}");
        }

        #endregion

        private static void setBallColor(IBall ball, State state)
        {
            if (state.isRedTeamActive && !state.isBlueTeamActive)
            {
                ball.setColor(BallColor.RedTeam);
                return;
            }
            if (state.isBlueTeamActive && !state.isRedTeamActive)
            {
                ball.setColor(BallColor.BlueTeam);
                return;
            }
            ball.setColor(BallColor.NoTeam);
        }
    }
}