using Examples.Config.Scripts;
using Examples.Game.Scripts.Battle.interfaces;
using Examples.Game.Scripts.Battle.Scene;
using Photon.Pun;
using System;
using UnityEngine;

namespace Examples.Game.Scripts.Battle.SlingShot
{
    /// <summary>
    ///  Puts the ball on the game using "slingshot" method between two team mates in positions A and B.
    /// </summary>
    /// <remarks>
    /// Team mate in position A is the player whose head was hit - the "take a catch" player.
    /// </remarks>
    public class BallHeadShot : MonoBehaviourPunCallbacks
    {
        [Header("Settings"), SerializeField] private float gapToBall;
        [SerializeField] private LineRenderer line;

        [Header("Live Data"), SerializeField] private Transform followA;
        [SerializeField] private Transform followB;
        [SerializeField] private Transform ballTransform;
        [SerializeField] private Vector3 a;
        [SerializeField] private Vector3 b;
        [SerializeField] private int teamIndex;
        [SerializeField] private float ballStartTime;

        [Header("Debug"), SerializeField] private Vector3 deltaBall;
        [SerializeField] private float distanceBall;

        private IBallControl ballControl;

        private void Awake()
        {
            enabled = false;
        }


        public override void OnPlayerLeftRoom(Photon.Realtime.Player otherPlayer)
        {
            // When any player leaves, the game is over!
            gameObject.SetActive(false);
        }

        private void Update()
        {
            a = followA.position;
            b = followB.position;
            // Track line
            line.SetPosition(0, b);
            line.SetPosition(1, a);
            // Track ball
            ballTransform.position = a + (a - b).normalized * distanceBall;

            if (Time.time > ballStartTime)
            {
                startBall();
            }
        }

        private void startBall()
        {
            var variables = RuntimeGameConfig.Get().variables;
            var delta = a - b;
            var direction = delta.normalized;
            var speed = Mathf.Clamp(Mathf.Abs(delta.magnitude), variables.minSlingShotDistance, variables.maxSlingShotDistance);
            startTheBall(ballControl, ballTransform.position, teamIndex, direction, speed);
            line.gameObject.SetActive(false);
            enabled = false;
        }

        public void restartBall(IBallControl ball, IPlayerActor playerA)
        {
            Debug.Log($"restartBall");
            ballControl = ball;
            ballTransform = ((Component)ballControl).transform;

            playerA.setGhostedMode();
            teamIndex = playerA.TeamIndex;
            followA = ((Component)playerA).transform;
            var teamMate = playerA.TeamMate;
            if (teamMate != null)
            {
                teamMate.setGhostedMode();
                followB = ((Component)teamMate).transform;
            }
            else
            {
                followB = SceneConfig.Get().ballAnchors[teamIndex];
            }
            ballControl.ghostBall();
            ballControl.moveBall(Vector2.zero, 0f);

            deltaBall = (ballTransform.position - followA.position) * (1f + gapToBall);
            distanceBall = Mathf.Abs(deltaBall.magnitude);

            ballStartTime = Time.time + RuntimeGameConfig.Get().variables.ballRestartDelay;

            line.gameObject.SetActive(true);
            enabled = true;
        }

        private static void startTheBall(IBallControl ballControl, Vector2 position, int teamIndex, Vector2 direction, float speed)
        {
            ballControl.teleportBall(position, teamIndex);
            ballControl.showBall();
            ballControl.moveBall(direction, speed);
        }
    }
}