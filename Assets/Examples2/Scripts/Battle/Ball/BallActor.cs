using Examples2.Scripts.Battle.interfaces;
using Photon.Pun;
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
    }

    [Serializable]
    internal class BallState
    {
        public BallColor ballColor;
        public bool isMoving;
    }

    internal class BallActor : MonoBehaviour, IPunObservable, IBall
    {
        private const float ballTeleportDistance = 1f;

        [SerializeField] private BallSettings settings;
        [SerializeField] private BallState state;

        [Header("Photon"), SerializeField] private Vector2 networkPosition;
        [SerializeField] private float networkLag;

        private PhotonView _photonView;
        private Rigidbody2D _rigidbody;

        private GameObject[] stateObjects;

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
                Debug.Log($"IGNORE trigger collision {otherGameObject.name} layer {otherGameObject.layer}");
                return;
            }
            Debug.Log($"UNHANDLED trigger collision {otherGameObject.name} layer {otherGameObject.layer}");
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
                Debug.Log($"IGNORE trigger exit {otherGameObject.name} layer {otherGameObject.layer}");
                return;
            }
            Debug.Log($"UNHANDLED trigger exit {otherGameObject.name} layer {otherGameObject.layer}");
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
                Debug.Log($"IGNORE collision {otherGameObject.name} layer {otherGameObject.layer}");
                return;
            }
            Debug.Log($"UNHANDLED collision {otherGameObject.name} layer {otherGameObject.layer}");
        }

        #endregion

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
    }
}