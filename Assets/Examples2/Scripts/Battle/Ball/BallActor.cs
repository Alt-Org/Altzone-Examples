using System;
using Examples2.Scripts.Battle.interfaces;
using Photon.Pun;
using TMPro;
using UnityConstants;
using UnityEngine;

namespace Examples2.Scripts.Battle.Ball
{
    [Serializable]
    internal class BallSettings
    {
        [Header("Ball Setup")] public GameObject _ballCollider;
        public GameObject _colorNoTeam;
        public GameObject _colorRedTeam;
        public GameObject _colorBlueTeam;
        public GameObject _colorGhosted;
        public GameObject _colorHidden;

        [Header("Layers")] public LayerMask _teamAreaMask;
        public LayerMask _headMask;
        public LayerMask _shieldMask;
        public LayerMask _brickMask;
        public LayerMask _wallMask;

        [Header("Ball Constraints")] public float _minBallSpeed;
        public float _maxBallSpeed;
    }

    [Serializable]
    internal class BallState
    {
        public BallColor _ballColor;
        public bool _isMoving;
    }

    [RequireComponent(typeof(PhotonView))]
    internal class BallActor : MonoBehaviour, IPunObservable, IBall, IBallCollision
    {
        private const float BallTeleportDistance = 1f;
        private const float CheckVelocityDelay = 0.5f;

        [SerializeField] private BallSettings _settings;
        [SerializeField] private BallState _state;

        [Header("Photon"), SerializeField] private Vector2 _networkPosition;
        [SerializeField] private float _networkLag;

        [Header("Debug"), SerializeField] private TMP_Text _ballInfo;
        private GameObject _ballInfoParent;

        private PhotonView _photonView;
        private Rigidbody2D _rigidbody;

        [SerializeField] private float _currentSpeed;
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
            Debug.Log($"Awake");
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
                _settings._colorNoTeam,
                _settings._colorRedTeam,
                _settings._colorBlueTeam,
                _settings._colorGhosted,
                _settings._colorHidden
            };
            _teamAreaMaskValue = _settings._teamAreaMask.value;
            _headMaskValue = _settings._headMask.value;
            _shieldMaskValue = _settings._shieldMask.value;
            _brickMaskValue = _settings._brickMask.value;
            _wallMaskValue = _settings._wallMask.value;
            _ballInfoParent = _ballInfo.gameObject;
        }

        private void OnEnable()
        {
            Debug.Log($"OnEnable IsMine {_photonView.IsMine}");
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
                _networkPosition = (Vector2)stream.ReceiveNext();
                _rigidbody.velocity = (Vector2)stream.ReceiveNext();

                _networkLag = Mathf.Abs((float)(PhotonNetwork.Time - info.SentServerTime));
                _networkPosition += _rigidbody.velocity * _networkLag;

                // Just for testing - this is expensive call!
                _ballInfo.text = _rigidbody.velocity.magnitude.ToString("F1");
            }
        }

        private void Update()
        {
            if (!_photonView.IsMine)
            {
                var position = _rigidbody.position;
                var isTeleport = Mathf.Abs(position.x - _networkPosition.x) > BallTeleportDistance ||
                                 Mathf.Abs(position.y - _networkPosition.y) > BallTeleportDistance;
                _rigidbody.position = isTeleport
                    ? _networkPosition
                    : Vector2.MoveTowards(position, _networkPosition, Time.deltaTime);
                return;
            }
            if (_isCheckVelocityTime && _checkVelocityTime > Time.time)
            {
                _isCheckVelocityTime = false;
                if (!Mathf.Approximately(_currentSpeed, _rigidbody.velocity.magnitude))
                {
                    Debug.Log("fix velocity");
                    KeepConstantVelocity();
                }
            }
            // Just for testing - this is expensive call!
            _ballInfo.text = _rigidbody.velocity.magnitude.ToString("F1");
        }

        private void KeepConstantVelocity()
        {
            _rigidbody.velocity = _rigidbody.velocity.normalized * _currentSpeed;
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
            Debug.Log($"stopMoving {_state._isMoving} <- {false}");
            _state._isMoving = false;
            if (_photonView.IsMine)
            {
                _settings._ballCollider.SetActive(false);
            }
            _currentSpeed = 0f;
            _rigidbody.velocity = Vector2.zero;
        }

        void IBall.StartMoving(Vector2 position, Vector2 velocity)
        {
            if (_photonView.IsMine)
            {
                // Hackish way to enable us
                enabled = true;
            }
            Debug.Log($"startMoving {_state._isMoving} <- {true} position {position} velocity {velocity}");
            _state._isMoving = true;
            if (_photonView.IsMine)
            {
                _settings._ballCollider.SetActive(true);

                _rigidbody.position = position;
                var speed = Mathf.Clamp(Mathf.Abs(velocity.magnitude), _settings._minBallSpeed, _settings._maxBallSpeed);
                _rigidbody.velocity = velocity.normalized * speed;
                _currentSpeed = _rigidbody.velocity.magnitude;
            }
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
            _stateObjects[(int)_state._ballColor].SetActive(false);
            _state._ballColor = ballColor;
            _stateObjects[(int)_state._ballColor].SetActive(true);
            _ballInfoParent.SetActive(ballColor != BallColor.Hidden);
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