using Examples2.Scripts.Battle.interfaces;
using Photon.Pun;
using Prg.Scripts.Common.Unity;
using System;
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

        [Header("Tags"), TagSelector] public string redTeamTag;
        [TagSelector] public string blueTeamTag;
    }

    [Serializable]
    internal class BallState
    {
        public BallColor ballColor;
        public bool isMoving;
    }

    internal class BallActor : MonoBehaviour, IPunObservable, IBall, IBallCollision
    {
        private const float ballTeleportDistance = 1f;

        [SerializeField] private BallSettings settings;
        [SerializeField] private BallState state;

        [Header("Photon"), SerializeField] private Vector2 networkPosition;
        [SerializeField] private float networkLag;

        private PhotonView _photonView;
        private Rigidbody2D _rigidbody;

        private GameObject[] stateObjects;

        private int teamAreaMaskValue;
        private int headMaskValue;
        private int shieldMaskValue;
        private int brickMaskValue;
        private int wallMaskValue;

        private Action<GameObject> _onHeadCollision;
        private Action<GameObject> _onShieldCollision;
        private Action<GameObject> _onBrickCollision;
        private Action<GameObject> _onWallCollision;
        private Action<TeamColor> _onEnterTeamArea;
        private Action<TeamColor> _onExitTeamArea;

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
            stateObjects = new[]
            {
                settings.colorNoTeam,
                settings.colorRedTeam,
                settings.colorBlueTeam,
                settings.colorGhosted,
                settings.colorHidden,
                settings.colorPlaceholder
            };
            teamAreaMaskValue = settings.teamAreaMask.value;
            headMaskValue = settings.headMask.value;
            shieldMaskValue = settings.shieldMask.value;
            brickMaskValue = settings.brickMask.value;
            wallMaskValue = settings.wallMask.value;
        }

        private void OnEnable()
        {
            _rigidbody.velocity = Vector2.zero;
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
                var isTeleport = Mathf.Abs(position.x - networkPosition.x) > ballTeleportDistance ||
                                 Mathf.Abs(position.y - networkPosition.y) > ballTeleportDistance;
                _rigidbody.position = isTeleport
                    ? networkPosition
                    : Vector2.MoveTowards(position, networkPosition, Time.deltaTime);
                return;
            }
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
            var colliderMask = 1 << layer;
            if (teamAreaMaskValue == (teamAreaMaskValue | colliderMask))
            {
                checkTeamArea(otherGameObject, settings, _onEnterTeamArea);
                return;
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
            var colliderMask = 1 << layer;
            if (teamAreaMaskValue == (teamAreaMaskValue | colliderMask))
            {
                checkTeamArea(otherGameObject, settings, _onExitTeamArea);
                return;
            }
            Debug.Log($"UNHANDLED trigger_exit {otherGameObject.name} layer {layer} {LayerMask.LayerToName(layer)}");
        }

        private static void checkTeamArea(GameObject gameObject, BallSettings settings, Action<TeamColor> action)
        {
            var teamColor = gameObject.CompareTag(settings.blueTeamTag) ? TeamColor.Blue
                : gameObject.CompareTag(settings.redTeamTag) ? TeamColor.Red
                : TeamColor.None;
            action.Invoke(teamColor);
        }

        private void OnCollisionEnter2D(Collision2D other)
        {
            if (!enabled)
            {
                return; // Collision events will be sent to disabled MonoBehaviours, to allow enabling Behaviours in response to collisions.
            }
            var otherGameObject = other.gameObject;
            var layer = otherGameObject.layer;
            if (layer == 0)
            {
                Debug.Log($"IGNORE collision_enter {otherGameObject.name} layer {otherGameObject.layer}");
                return;
            }
            var colliderMask = 1 << layer;
            if (callbackEvent(headMaskValue, colliderMask, otherGameObject, _onHeadCollision))
            {
                return;
            }
            if (callbackEvent(shieldMaskValue, colliderMask, otherGameObject, _onShieldCollision))
            {
                return;
            }
            if (callbackEvent(brickMaskValue, colliderMask, otherGameObject, _onBrickCollision))
            {
                return;
            }
            if (callbackEvent(wallMaskValue, colliderMask, otherGameObject, _onWallCollision))
            {
                return;
            }
            Debug.Log($"UNHANDLED collision_enter {otherGameObject.name} layer {layer} {LayerMask.LayerToName(layer)}");
        }

        private static bool callbackEvent(int maskValue, int colliderMask, GameObject gameObject, Action<GameObject> callback)
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

        IBallCollision IBall.ballCollision => this;

        void IBall.stopMoving()
        {
            Debug.Log($"stopMoving {state.isMoving} <- {false}");
            state.isMoving = false;
            settings.ballCollider.SetActive(false);
            _rigidbody.velocity = Vector2.zero;
        }

        void IBall.startMoving(Vector2 position, Vector2 velocity)
        {
            Debug.Log($"startMoving {state.isMoving} <- {true} position {position} velocity {velocity}");
            state.isMoving = true;
            settings.ballCollider.SetActive(true);
            _rigidbody.position = position;
            _rigidbody.velocity = velocity;
        }

        void IBall.setColor(BallColor ballColor)
        {
            Debug.Log($"setColor {state.ballColor} <- {ballColor}");
            stateObjects[(int)state.ballColor].SetActive(false);
            state.ballColor = ballColor;
            stateObjects[(int)state.ballColor].SetActive(true);
        }

        #endregion

        #region IBallCollision

        Action<GameObject> IBallCollision.onHeadCollision
        {
            get => _onHeadCollision;
            set => _onHeadCollision = value;
        }

        Action<GameObject> IBallCollision.onShieldCollision
        {
            get => _onShieldCollision;
            set => _onShieldCollision = value;
        }

        Action<GameObject> IBallCollision.onWallCollision
        {
            get => _onWallCollision;
            set => _onWallCollision = value;
        }

        Action<GameObject> IBallCollision.onBrickCollision
        {
            get => _onBrickCollision;
            set => _onBrickCollision = value;
        }

        Action<TeamColor> IBallCollision.onEnterTeamArea
        {
            get => _onEnterTeamArea;
            set => _onEnterTeamArea = value;
        }

        Action<TeamColor> IBallCollision.onExitTeamArea
        {
            get => _onExitTeamArea;
            set => _onExitTeamArea = value;
        }

        #endregion
    }
}