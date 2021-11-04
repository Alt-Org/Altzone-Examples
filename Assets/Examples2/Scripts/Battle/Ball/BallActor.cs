using Examples2.Scripts.Battle.interfaces;
using Photon.Pun;
using System;
using TMPro;
using UnityConstants;
using UnityEngine;

namespace Examples2.Scripts.Battle.Ball
{
    [Serializable]
    internal class BallSettings
    {
        [Header("Ball Setup")] public GameObject ballCollider;
        public GameObject colorNoTeam;
        public GameObject colorRedTeam;
        public GameObject colorBlueTeam;
        public GameObject colorGhosted;
        public GameObject colorHidden;
        public GameObject colorPlaceholder;

        [Header("Layers")] public LayerMask teamAreaMask;
        public LayerMask headMask;
        public LayerMask shieldMask;
        public LayerMask brickMask;
        public LayerMask wallMask;

        [Header("Ball Constraints")] public float minBallSpeed;
        public float maxBallSpeed;
    }

    [Serializable]
    internal class BallState
    {
        public BallColor ballColor;
        public bool isMoving;
    }

    internal class BallActor : MonoBehaviour, IPunObservable, IBall, IBallCollision
    {
        private const float BallTeleportDistance = 1f;
        private const float CheckVelocityDelay = 0.5f;

        [SerializeField] private BallSettings settings;
        [SerializeField] private BallState state;

        [Header("Photon"), SerializeField] private Vector2 networkPosition;
        [SerializeField] private float networkLag;

        [Header("Debug"), SerializeField] private TMP_Text ballInfo;

        private PhotonView _photonView;
        private Rigidbody2D _rigidbody;

        [SerializeField] private float currentSpeed;
        private bool _isCheckVelocityTime;
        private float _checkVelocityTime;

        private GameObject[] _stateObjects;

        private int _teamAreaMaskValue;
        private int _headMaskValue;
        private int _shieldMaskValue;
        private int _brickMaskValue;
        private int _wallMaskValue;

        private Action<GameObject> _onHeadCollision;
        private Action<GameObject> _onShieldCollision;
        private Action<GameObject> _onBrickCollision;
        private Action<GameObject> _onWallCollision;
        private Action<GameObject> _onEnterTeamArea;
        private Action<GameObject> _onExitTeamArea;

        private void Awake()
        {
            _photonView = PhotonView.Get(this);
            if (!_photonView.ObservedComponents.Contains(this))
            {
                // If not set in Editor
                _photonView.ObservedComponents.Add(this);
            }
            _rigidbody = GetComponent<Rigidbody2D>();
            _rigidbody.isKinematic = !_photonView.IsMine;
            _stateObjects = new[] // This is indexed by BallColor!
            {
                settings.colorPlaceholder,
                settings.colorNoTeam,
                settings.colorRedTeam,
                settings.colorBlueTeam,
                settings.colorGhosted,
                settings.colorHidden
            };
            _teamAreaMaskValue = settings.teamAreaMask.value;
            _headMaskValue = settings.headMask.value;
            _shieldMaskValue = settings.shieldMask.value;
            _brickMaskValue = settings.brickMask.value;
            _wallMaskValue = settings.wallMask.value;
        }

        private void OnEnable()
        {
            if (_photonView.IsMine)
            {
                var ball = (IBall)this;
                ball.StopMoving();
                ball.SetColor(BallColor.Hidden);
            }
        }

        #region Movement

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
                var position = _rigidbody.position;
                var isTeleport = Mathf.Abs(position.x - networkPosition.x) > BallTeleportDistance ||
                                 Mathf.Abs(position.y - networkPosition.y) > BallTeleportDistance;
                _rigidbody.position = isTeleport
                    ? networkPosition
                    : Vector2.MoveTowards(position, networkPosition, Time.deltaTime);
                return;
            }
            if (_isCheckVelocityTime && _checkVelocityTime > Time.time)
            {
                _isCheckVelocityTime = false;
                if (!Mathf.Approximately(currentSpeed, _rigidbody.velocity.magnitude))
                {
                    Debug.Log("fix velocity");
                    KeepConstantVelocity();
                }
            }
            // Just for testing - this is expensive call!
            ballInfo.text = _rigidbody.velocity.magnitude.ToString("F1");
        }

        private void KeepConstantVelocity()
        {
            _rigidbody.velocity = _rigidbody.velocity.normalized * currentSpeed;
        }

        #endregion

        #region Collisions

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (!enabled)
            {
                return; // Collision events will be sent to disabled MonoBehaviours, to allow enabling Behaviours in response to collisions.
            }
            var otherGameObject = other.gameObject;
            var layer = otherGameObject.layer;
            if (layer == 0)
            {
                Debug.Log($"IGNORE trigger_enter {otherGameObject.name} layer {otherGameObject.layer}");
                return;
            }
            if (!otherGameObject.CompareTag(Tags.Untagged))
            {
                var colliderMask = 1 << layer;
                if (CallbackEvent(_teamAreaMaskValue, colliderMask, otherGameObject, _onEnterTeamArea))
                {
                    return;
                }
            }
            Debug.Log($"UNHANDLED trigger_enter {otherGameObject.name} layer {layer} {LayerMask.LayerToName(layer)}");
        }

        private void OnTriggerExit2D(Collider2D other)
        {
            if (!enabled)
            {
                return; // Collision events will be sent to disabled MonoBehaviours, to allow enabling Behaviours in response to collisions.
            }
            var otherGameObject = other.gameObject;
            var layer = otherGameObject.layer;
            if (layer == 0)
            {
                Debug.Log($"IGNORE trigger_exit {otherGameObject.name} layer {otherGameObject.layer}");
                return;
            }
            if (!otherGameObject.CompareTag(Tags.Untagged))
            {
                var colliderMask = 1 << layer;
                if (CallbackEvent(_teamAreaMaskValue, colliderMask, otherGameObject, _onExitTeamArea))
                {
                    return;
                }
            }
            Debug.Log($"UNHANDLED trigger_exit {otherGameObject.name} layer {layer} {LayerMask.LayerToName(layer)}");
        }

        private void OnCollisionEnter2D(Collision2D other)
        {
            if (!enabled)
            {
                return; // Collision events will be sent to disabled MonoBehaviours, to allow enabling Behaviours in response to collisions.
            }
            _isCheckVelocityTime = true;
            _checkVelocityTime = Time.time + CheckVelocityDelay;
            var otherGameObject = other.gameObject;
            var layer = otherGameObject.layer;
            if (layer == 0)
            {
                Debug.Log($"IGNORE collision_enter {otherGameObject.name} layer {otherGameObject.layer}");
                return;
            }
            var colliderMask = 1 << layer;
            if (CallbackEvent(_headMaskValue, colliderMask, otherGameObject, _onHeadCollision))
            {
                return;
            }
            if (CallbackEvent(_shieldMaskValue, colliderMask, otherGameObject, _onShieldCollision))
            {
                return;
            }
            if (CallbackEvent(_brickMaskValue, colliderMask, otherGameObject, _onBrickCollision))
            {
                return;
            }
            if (IsCallbackEvent(_wallMaskValue, colliderMask))
            {
                if (!otherGameObject.CompareTag(Tags.Untagged))
                {
                    _onWallCollision?.Invoke(otherGameObject);
                }
                return;
            }
            Debug.Log($"UNHANDLED collision_enter {otherGameObject.name} layer {layer} {LayerMask.LayerToName(layer)}");
        }

        private static bool IsCallbackEvent(int maskValue, int colliderMask)
        {
            return maskValue == (maskValue | colliderMask);
        }

        private static bool CallbackEvent(int maskValue, int colliderMask, GameObject gameObject, Action<GameObject> callback)
        {
            if (maskValue == (maskValue | colliderMask))
            {
                callback?.Invoke(gameObject);
                return true;
            }
            return false;
        }

        #endregion

        #region IBall

        IBallCollision IBall.BallCollision => this;

        void IBall.StopMoving()
        {
            Debug.Log($"stopMoving {state.isMoving} <- {false}");
            state.isMoving = false;
            if (_photonView.IsMine)
            {
                settings.ballCollider.SetActive(false);
            }
            currentSpeed = 0f;
            _rigidbody.velocity = Vector2.zero;
        }

        void IBall.StartMoving(Vector2 position, Vector2 velocity)
        {
            Debug.Log($"startMoving {state.isMoving} <- {true} position {position} velocity {velocity}");
            state.isMoving = true;
            if (_photonView.IsMine)
            {
                settings.ballCollider.SetActive(true);
            }
            _rigidbody.position = position;
            var speed = Mathf.Clamp(Mathf.Abs(velocity.magnitude), settings.minBallSpeed, settings.maxBallSpeed);
            _rigidbody.velocity = velocity.normalized * speed;
            currentSpeed = _rigidbody.velocity.magnitude;
        }

        void IBall.SetColor(BallColor ballColor)
        {
            if (_photonView.IsMine)
            {
                _photonView.RPC(nameof(SetBallColorRpc), RpcTarget.All, (byte)ballColor);
            }
        }

        [PunRPC]
        private void SetBallColorRpc(byte ballColor)
        {
            _setBallColorLocal((BallColor)ballColor);
        }

        private void _setBallColorLocal(BallColor ballColor)
        {
            //Debug.Log($"setColor {state.ballColor} <- {ballColor}");
            _stateObjects[(int)state.ballColor].SetActive(false);
            state.ballColor = ballColor;
            _stateObjects[(int)state.ballColor].SetActive(true);
        }

        #endregion

        #region IBallCollision

        Action<GameObject> IBallCollision.OnHeadCollision
        {
            get => _onHeadCollision;
            set => _onHeadCollision = value;
        }

        Action<GameObject> IBallCollision.OnShieldCollision
        {
            get => _onShieldCollision;
            set => _onShieldCollision = value;
        }

        Action<GameObject> IBallCollision.OnWallCollision
        {
            get => _onWallCollision;
            set => _onWallCollision = value;
        }

        Action<GameObject> IBallCollision.OnBrickCollision
        {
            get => _onBrickCollision;
            set => _onBrickCollision = value;
        }

        Action<GameObject> IBallCollision.OnEnterTeamArea
        {
            get => _onEnterTeamArea;
            set => _onEnterTeamArea = value;
        }

        Action<GameObject> IBallCollision.OnExitTeamArea
        {
            get => _onExitTeamArea;
            set => _onExitTeamArea = value;
        }

        #endregion
    }
}