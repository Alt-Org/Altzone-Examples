using System;
using Altzone.Scripts.Battle;
using Examples2.Scripts.Battle.Factory;
using Examples2.Scripts.Battle.interfaces;
using Examples2.Scripts.Battle.Room;
using Prg.Scripts.Common.PubSub;
using UnityConstants;
using UnityEditor;
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

        private void OnHeadCollision(Collision2D collision)
        {
            var other = collision.gameObject;
            Debug.Log($"onHeadCollision {other.GetFullPath()}");
            var playerActor = other.GetComponentInParent<IPlayerActor>();
            playerActor.HeadCollision();
            var scoreType = playerActor.TeamNumber == PhotonBattle.TeamBlueValue ? ScoreType.RedHead : ScoreType.BlueHead;
            this.Publish(new ScoreManager.ScoreEvent(scoreType));
        }

        private void OnShieldCollision(Collision2D collision)
        {
            if (collision.contactCount == 0)
            {
                return;
            }
            var contactPoint = collision.GetContact(0);
            var other = collision.gameObject;
            Debug.Log($"onShieldCollision {other.GetFullPath()} @ point {contactPoint.point}");
            var playerActor = other.GetComponentInParent<IPlayerActor>();
            playerActor.ShieldCollision(contactPoint.point);
        }

        private void OnBrickCollision(Collision2D collision)
        {
            //Debug.Log($"onBrickCollision {other.name}");
            _brickManager.DeleteBrick(collision.gameObject);
        }

        private void OnWallCollision(Collision2D collision)
        {
            var other = collision.gameObject;
            Debug.Log($"onWallCollision {other.name} {other.tag}");
            if (other.CompareTag(Tags.BlueTeam))
            {
                this.Publish(new ScoreManager.ScoreEvent(ScoreType.BlueWall));
            }
            else if (other.CompareTag(Tags.RedTeam))
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