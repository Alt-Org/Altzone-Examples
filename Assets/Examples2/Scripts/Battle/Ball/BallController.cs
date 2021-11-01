using Photon.Pun;
using System;
using UnityEngine;

namespace Examples2.Scripts.Battle.Ball
{
    internal class BallController : MonoBehaviour, IPunObservable
    {
        private const float ballTeleportDistance = 1f;
        private const float ballLerpSmoothingFactor = 4f;

        [Header("Movement"), SerializeField] private Vector2 targetVelocity;
        [SerializeField] private bool isMoving;

        [Header("Photon"), SerializeField] private Vector2 networkPosition;
        [SerializeField] private float networkLag;

        private PhotonView _photonView;
        private Rigidbody2D _rigidbody;

        private void Awake()
        {
            _photonView = PhotonView.Get(this);
            _rigidbody = GetComponent<Rigidbody2D>();
        }

        private void OnEnable()
        {
            _rigidbody.velocity = Vector2.zero;
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
                var position = _rigidbody.position;
                var isTeleport = Mathf.Abs(position.x - networkPosition.x) > ballTeleportDistance ||
                                 Mathf.Abs(position.y - networkPosition.y) > ballTeleportDistance;
                _rigidbody.position = isTeleport
                    ? networkPosition
                    : Vector2.MoveTowards(position, networkPosition, Time.deltaTime);
                return;
            }
            if (isMoving)
            {
                _rigidbody.velocity = targetVelocity;
                isMoving = false;
            }
        }

        private void FixedUpdate()
        {
            if (!_photonView.IsMine)
            {
                return;
            }
        }

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
    }
}