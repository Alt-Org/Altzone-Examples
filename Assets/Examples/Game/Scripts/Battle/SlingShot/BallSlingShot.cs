using Examples.Config.Scripts;
using Examples.Game.Scripts.Battle.Ball;
using Examples.Game.Scripts.Battle.interfaces;
using Examples.Game.Scripts.Battle.Player;
using Examples.Game.Scripts.Battle.Scene;
using Photon.Pun;
using Prg.Scripts.Common.Photon;
using System.Linq;
using UnityEngine;

namespace Examples.Game.Scripts.Battle.SlingShot
{
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

        [Header("Settings"), SerializeField] private int teamIndex;
        [SerializeField] private SpriteRenderer spriteA;
        [SerializeField] private SpriteRenderer spriteB;
        [SerializeField] private LineRenderer line;

        [Header("Live Data"), SerializeField] private Transform followA;
        [SerializeField] private Transform followB;
        [SerializeField] private Vector3 a;
        [SerializeField] private Vector3 b;

        [Header("Debug"), SerializeField] private Vector2 position;
        [SerializeField] private Vector2 direction;
        [SerializeField] private float _currentDistance;

        private IBallControl ballControl;
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
            ballControl = BallActor.Get();
            ballControl.hideBall();

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
            startTheBall(ballControl, position, teamIndex, direction, _currentDistance); // Ball takes care of its own network synchronization
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
            var variables = RuntimeGameConfig.Get().variables;
            _currentDistance = Mathf.Clamp(direction.magnitude, variables.minSlingShotDistance, variables.maxSlingShotDistance);
            direction = direction.normalized;
        }

        private static void startTheBall(IBallControl ballControl, Vector2 position, int teamIndex, Vector2 direction, float speed)
        {
            ballControl.teleportBall(position, teamIndex);
            ballControl.showBall();
            ballControl.moveBall(direction, speed);
        }

        public static void startTheBall()
        {
            // Get slingshot with longest distance and start it.
            var ballSlingShot = FindObjectsOfType<BallSlingShot>()
                .Cast<IBallSlingShot>()
                .OrderByDescending(x => x.currentDistance)
                .FirstOrDefault();

            ballSlingShot?.startBall();

            // HACK to set players on the game after ball has been started!
            var ball = BallActor.Get();
            var ballSideTeam = ball.currentTeamIndex;
            foreach (var playerActor in PlayerActivator.allPlayerActors)
            {
                if (playerActor.TeamIndex == ballSideTeam)
                {
                    playerActor.setFrozenMode();
                }
                else
                {
                    playerActor.setNormalMode();
                }
            }
        }
    }
}