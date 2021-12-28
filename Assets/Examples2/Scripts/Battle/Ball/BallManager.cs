using System;
using Altzone.Scripts.Battle;
using Examples2.Scripts.Battle.Factory;
using Examples2.Scripts.Battle.interfaces;
using Examples2.Scripts.Battle.Room;
using Prg.Scripts.Common.PubSub;
using UnityConstants;
using UnityEngine;

namespace Examples2.Scripts.Battle.Ball
{
    /// <summary>
    /// <c>BallManager</c> listens events from the <c>IBall</c> and forwards them where applicable - even to the <c>IBall</c> itself.
    /// </summary>
    public class BallManager : MonoBehaviour
    {
        [Serializable]
        internal class State
        {
            public bool _isRedTeamActive;
            public bool _isBlueTeamActive;
        }

        [SerializeField] private State _state;

        private IBall _ball;
        private IBrickManager _brickManager;

        private void Awake()
        {
            Debug.Log("Awake");
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

        #region IBallCollision callback events

        private void OnHeadCollision(GameObject other)
        {
            Debug.Log($"onHeadCollision {other.name}");
            var playerActor = other.GetComponentInParent<IPlayerActor>();
            playerActor.HeadCollision();
            this.Publish(new ScoreManager.ScoreEvent(ScoreType.PlayerHed));
        }

        private void OnShieldCollision(GameObject other)
        {
            Debug.Log($"onShieldCollision {other.name}");
            var playerActor = other.GetComponentInParent<IPlayerActor>();
            playerActor.ShieldCollision();
        }

        private void OnBrickCollision(GameObject other)
        {
            //Debug.Log($"onBrickCollision {other.name}");
            _brickManager.DeleteBrick(other);
        }

        private void OnWallCollision(GameObject other)
        {
            Debug.Log($"onWallCollision {other.name} {other.tag}");
            if (other.CompareTag(Tags.BlueTeam))
            {
                this.Publish(new ScoreManager.ScoreEvent(ScoreType.BlueWall));
            }
            else if (other.CompareTag(Tags.BlueTeam))
            {
                this.Publish(new ScoreManager.ScoreEvent(ScoreType.RedWall));
            }
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

        private static void SetBallColor(IBall ball, State state)
        {
            if (state._isRedTeamActive && !state._isBlueTeamActive)
            {
                ball.SetColor(BallColor.RedTeam);
                ball.Publish(new ActiveTeamEvent(PhotonBattle.TeamRedValue));
                return;
            }
            if (state._isBlueTeamActive && !state._isRedTeamActive)
            {
                ball.SetColor(BallColor.BlueTeam);
                ball.Publish(new ActiveTeamEvent(PhotonBattle.TeamBlueValue));
                return;
            }
            ball.SetColor(BallColor.NoTeam);
            ball.Publish(new ActiveTeamEvent(PhotonBattle.NoTeamValue));
        }

        #endregion

        /// <summary>
        /// Active team event is sent whenever active team is changed.
        /// </summary>
        /// <remarks>
        /// <c>TeamIndex</c> is -1 when no team is active.
        /// </remarks>
        internal class ActiveTeamEvent
        {
            public readonly int TeamIndex;

            public ActiveTeamEvent(int teamIndex)
            {
                TeamIndex = teamIndex;
            }
        }
    }
}