using Examples2.Scripts.Battle.Factory;
using Examples2.Scripts.Battle.interfaces;
using Photon.Pun;
using System;
using System.Collections;
using UnityEngine;

namespace Examples2.Scripts.Battle.Ball
{

    internal class BallController : MonoBehaviour
    {
        [Serializable]
        internal class State
        {
            public bool isBallMoving;
            public bool isRedTeamActive;
            public bool isBlueTeamActive;
        }

        [SerializeField] private State state;

        [Header("Debug Only")] public Vector2 ballVelocity;
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
                state.isBallMoving = true;
                ball.setColor(BallColor.NoTeam);
                ball.startMoving(ballPosition, ballVelocity);
                return;
            }
            if (stopBallMoving)
            {
                stopBallMoving = false;
                state.isBallMoving = false;
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
            Destroy(other);
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