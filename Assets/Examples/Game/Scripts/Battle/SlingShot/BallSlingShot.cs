﻿using Examples.Game.Scripts.Battle.Ball;
using Examples.Game.Scripts.Battle.Player;
using Examples.Game.Scripts.Battle.Scene;
using Photon.Pun;
using Prg.Scripts.Common.Photon;
using System.Linq;
using UnityEngine;

namespace Examples.Game.Scripts.Battle.SlingShot
{
    /// <summary>
    /// Interface to start the ball aka put the ball into play by sling shot.
    /// </summary>
    public interface IBallSlingShot
    {
        void startBall();
        float currentDistance { get; }
    }

    /// <summary>
    ///  Puts the ball on the game using "sling shot" method between two team mates in position A and B.
    /// </summary>
    /// <remarks>
    /// Position A is end point of aiming and position B is start point of aiming.<br />
    /// Vector A-B provides direction and relative speed (increase or decrease) to the ball when it is started to the game.
    /// </remarks>
    public class BallSlingShot : MonoBehaviourPunCallbacks, IBallSlingShot
    {
        private const int msgHideSlingShot = PhotonEventDispatcher.eventCodeBase + 2;

        private const float minDistance = 3f;
        private const float maxDistance = 9f;

        [Header("Settings"), SerializeField] private int teamIndex;
        [SerializeField] private SpriteRenderer spriteA;
        [SerializeField] private SpriteRenderer spriteB;
        [SerializeField] private LineRenderer line;

        [Header("Live Data"), SerializeField] private BallActor ballActor;
        [SerializeField] private Transform followA;
        [SerializeField] private Transform followB;
        [SerializeField] private Vector3 a;
        [SerializeField] private Vector3 b;

        [Header("Debug"), SerializeField] private Vector2 position;
        [SerializeField] private Vector2 direction;
        [SerializeField] private float _currentDistance;

        private PhotonEventDispatcher photonEventDispatcher;

        private void Awake()
        {
            Debug.Log("Awake");
            photonEventDispatcher = PhotonEventDispatcher.Get();
            photonEventDispatcher.registerEventListener(msgHideSlingShot, data => { onHideSlingShot(); });
        }

        private void sendHideSlingShot()
        {
            photonEventDispatcher.RaiseEvent(msgHideSlingShot, null);
        }

        private void onHideSlingShot()
        {
            gameObject.SetActive(false);
        }

        public override void OnEnable()
        {
            base.OnEnable();
            var playerActors = FindObjectsOfType<PlayerActor>()
                .Cast<IPlayerActor>()
                .Where(x => x.TeamIndex == teamIndex)
                .OrderBy(x => x.PlayerPos)
                .ToList();
            Debug.Log($"OnEnable team={teamIndex} playerActors={playerActors.Count}");
            if (playerActors.Count == 0)
            {
                gameObject.SetActive(false); // No players for our team!
                return;
            }
            // Hide ball immediately
            ballActor = FindObjectOfType<BallActor>();
            ((IBallControl)ballActor).hideBall();

            followA = ((PlayerActor)playerActors[0]).transform;
            if (playerActors.Count == 2)
            {
                followB = ((PlayerActor)playerActors[1]).transform;
            }
            else
            {
                var teamMatePos = playerActors[0].TeamMatePos;
                followB = SceneConfig.Get().playerStartPos[teamMatePos]; // Never moves
            }
            // LineRenderer should be configured ok in Editor - we just move both "ends" on the fly!
            line.positionCount = 2;
        }

        public override void OnPlayerLeftRoom(Photon.Realtime.Player otherPlayer)
        {
            // When any player leaves, the game is over!
            gameObject.SetActive(false);
        }

        void IBallSlingShot.startBall()
        {
            starTheBall(ballActor, position, teamIndex, direction, _currentDistance); // Ball takes care of its own network synchronization
            sendHideSlingShot();
        }

        float IBallSlingShot.currentDistance => _currentDistance;

        private void Update()
        {
            a = followA.position;
            b = followB.position;

            spriteA.transform.position = a;
            spriteB.transform.position = b;
            line.SetPosition(0, a);
            line.SetPosition(1, b);

            position = a;
            direction = b - a;
            _currentDistance = Mathf.Clamp(direction.magnitude, minDistance, maxDistance);
            direction = direction.normalized;
        }

        private static void starTheBall(IBallControl ballControl, Vector2 position, int teamIndex, Vector2 direction, float speed)
        {
            ballControl.teleportBall(position, teamIndex);
            ballControl.showBall();
            ballControl.moveBall(direction, speed);
        }
    }
}