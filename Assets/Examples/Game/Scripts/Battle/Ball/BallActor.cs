using Examples.Config.Scripts;
using Examples.Game.Scripts.Battle.interfaces;
using Examples.Game.Scripts.Battle.Player;
using Examples.Game.Scripts.Battle.Room;
using Photon.Pun;
using Prg.Scripts.Common.PubSub;
using UnityEngine;

namespace Examples.Game.Scripts.Battle.Ball
{
    /// <summary>
    /// Simple ball with <c>Rigidbody2D</c> that synchronizes its movement across network using <c>PhotonView</c> and <c>RPC</c>.
    /// </summary>
    public class BallActor : MonoBehaviour, IPunObservable, IBallControl
    {
        public static IBallControl Get()
        {
            if (_Instance == null)
            {
                _Instance = FindObjectOfType<BallActor>();
            }
            return _Instance;
        }

        private static IBallControl _Instance;

        [Header("Settings"), SerializeField] private SpriteRenderer _sprite;
        [SerializeField] private Collider2D _collider;

        [SerializeField] private LayerMask collisionToHeadMask;
        [SerializeField] private int collisionToHead;
        [SerializeField] private LayerMask collisionToWallMask;
        [SerializeField] private int collisionToWall;
        [SerializeField] private LayerMask collisionToBrickMask;
        [SerializeField] private int collisionToBrick;

        [Header("Live Data"), SerializeField] private int _curTeamIndex;
        [SerializeField] private float targetSpeed;
        [SerializeField] private BallCollision ballCollision;

        [Header("Photon"), SerializeField] private Vector2 networkPosition;
        [SerializeField] private float networkLag;

        private Rigidbody2D _rigidbody;
        private PhotonView _photonView;

        public float collidersDisabledTime; // Ball colliders will be disabled after ball is started to avoid player over the ball!
        private bool isBallStarting;

        private float ballGracePeriod;

        // Configurable settings
        private GameVariables variables;

        private void Awake()
        {
            Debug.Log("Awake");
            variables = RuntimeGameConfig.Get().variables;
            _rigidbody = GetComponent<Rigidbody2D>();
            _photonView = PhotonView.Get(this);
            _rigidbody.isKinematic = !_photonView.IsMine;
            _collider.enabled = false;

            collisionToHead = collisionToHeadMask.value;
            collisionToWall = collisionToWallMask.value;
            collisionToBrick = collisionToBrickMask.value;

            _curTeamIndex = -1;
            targetSpeed = 0;
            ballCollision = gameObject.AddComponent<BallCollision>();
            ballCollision.enabled = false;
            ((IBallCollisionSource)ballCollision).onCurrentTeamChanged = onCurrentTeamChanged;
            ((IBallCollisionSource)ballCollision).onCollision2D = onBallCollision;
        }

        private void OnDestroy()
        {
            _Instance = null;
        }

        private void onCurrentTeamChanged(int newTeamIndex)
        {
            Debug.Log($"onCurrentTeamChanged ({_curTeamIndex}) <- ({newTeamIndex})");
            _curTeamIndex = newTeamIndex;
            this.Publish(new ActiveTeamEvent(newTeamIndex));
        }

        private void onBallCollision(Collision2D other)
        {
            var otherGameObject = other.gameObject;
            var colliderMask = 1 << otherGameObject.layer;
            if (collisionToBrick == (collisionToBrick | colliderMask))
            {
                BrickManager.deleteBrick(other.gameObject);
                return;
            }
            if (collisionToHead == (collisionToHead | colliderMask))
            {
                // Contract: player is one level up from head collider
                var playerActor = otherGameObject.GetComponentInParent<PlayerActor>() as IPlayerActor;
                playerActor.headCollision();
                return;
            }
            if (collisionToWall == (collisionToWall | colliderMask))
            {
                ScoreManager.addWallScore(other.gameObject);
                return;
            }
            Debug.Log($"onBallCollision UNHANDLED team={_curTeamIndex} other={other.gameObject.name}");
        }

        private void OnEnable()
        {
            Debug.Log("OnEnable");
            ((IBallControl)this).showBall();
        }

        private void OnDisable()
        {
            Debug.Log("OnDisable");
            if (PhotonNetwork.InRoom)
            {
                ((IBallControl)this).hideBall();
            }
        }

        int IBallControl.currentTeamIndex => _curTeamIndex;

        void IBallControl.teleportBall(Vector2 position, int teamIndex)
        {
            onCurrentTeamChanged(teamIndex);
            _rigidbody.position = position;
            if (collidersDisabledTime > 0f)
            {
                isBallStarting = true;
                _collider.enabled = false;
                ballGracePeriod = Time.time + collidersDisabledTime;
            }
            else
            {
                isBallStarting = false;
                _collider.enabled = true;
                ballGracePeriod = 0f;
            }
        }

        void IBallControl.moveBall(Vector2 direction, float speed)
        {
            targetSpeed = speed;
            _rigidbody.velocity = direction.normalized * speed;
            Debug.Log($"moveBall position={_rigidbody.position} velocity={_rigidbody.velocity} speed={targetSpeed}");
        }

        void IBallControl.showBall()
        {
            _photonView.RPC(nameof(setBallVisibilityRpc), RpcTarget.All, true);
        }

        void IBallControl.hideBall()
        {
            _photonView.RPC(nameof(setBallVisibilityRpc), RpcTarget.All, false);
        }

        private void _showBall()
        {
            ballCollision.enabled = true;
            _sprite.enabled = true;
            if (isBallStarting)
            {
                _collider.enabled = false;
            }
            else
            {
                _collider.enabled = true;
            }
            Debug.Log($"showBall position={_rigidbody.position} and isBallStarting={isBallStarting}");
        }

        private void _hideBall()
        {
            ballCollision.enabled = false;
            _sprite.enabled = false;
            _collider.enabled = false;
            targetSpeed = 0;
            _rigidbody.velocity = Vector2.zero;
            Debug.Log($"hideBall position={_rigidbody.position}");
        }

        void IPunObservable.OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
        {
            if (stream.IsWriting)
            {
                stream.SendNext(_rigidbody.position);
                stream.SendNext(_rigidbody.velocity);
            }
            else
            {
                networkPosition = (Vector2)stream.ReceiveNext();
                _rigidbody.velocity = (Vector2)stream.ReceiveNext();

                networkLag = Mathf.Abs((float)(PhotonNetwork.Time - info.SentServerTime));
                networkPosition += _rigidbody.velocity * networkLag;
            }
        }

        private void Update()
        {
            if (!_photonView.IsMine)
            {
                var curPos = _rigidbody.position;
                var deltaX = Mathf.Abs(curPos.x - networkPosition.x);
                var deltaY = Mathf.Abs(curPos.y - networkPosition.y);
                if (deltaX > variables.ballTeleportDistance || deltaY > variables.ballTeleportDistance)
                {
                    _rigidbody.position = networkPosition;
                }
                else
                {
                    _rigidbody.position = Vector2.MoveTowards(curPos, networkPosition, Time.deltaTime);
                }
                return;
            }
            if (isBallStarting)
            {
                if (Time.time > ballGracePeriod)
                {
                    isBallStarting = false;
                    _collider.enabled = true;
                    Debug.Log($"ball grace period end and collider is enabled");
                }
            }
        }

        private void FixedUpdate()
        {
            if (!_photonView.IsMine)
            {
                return;
            }
            if (targetSpeed > 0)
            {
                keepConstantVelocity(Time.fixedDeltaTime);
            }
        }

        private void keepConstantVelocity(float deltaTime)
        {
            var _velocity = _rigidbody.velocity;
            var targetVelocity = _velocity.normalized * targetSpeed;
            if (targetVelocity == Vector2.zero)
            {
                randomReset(_curTeamIndex);
                return;
            }
            if (targetVelocity != _rigidbody.velocity)
            {
                _rigidbody.velocity = Vector2.Lerp(_velocity, targetVelocity, deltaTime * variables.ballLerpSmoothingFactor);
            }
        }

        private void randomReset(int forTeam)
        {
            transform.position = Vector3.zero;
            var direction = forTeam == 0 ? Vector2.up : Vector2.down;
            _rigidbody.velocity = direction * targetSpeed;
        }

        [PunRPC]
        private void setBallVisibilityRpc(bool isVisible)
        {
            if (isVisible)
            {
                _showBall();
            }
            else
            {
                _hideBall();
            }
        }

        internal class ActiveTeamEvent
        {
            public readonly int newTeamIndex;

            public ActiveTeamEvent(int newTeamIndex)
            {
                this.newTeamIndex = newTeamIndex;
            }

            public override string ToString()
            {
                return $"team: {newTeamIndex}";
            }
        }
    }
}