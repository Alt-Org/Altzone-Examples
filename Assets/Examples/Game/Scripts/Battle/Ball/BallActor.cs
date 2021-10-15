﻿using Examples.Config.Scripts;
using Photon.Pun;
using UnityEngine;

namespace Examples.Game.Scripts.Battle.Ball
{
    public interface IBallControl
    {
        void teleportBall(Vector2 position);
        void moveBall(Vector2 direction, float speed);
        void showBall();
        void hideBall();
    }

    public class BallActor : MonoBehaviour, IPunObservable, IBallControl
    {
        [Header("Live Data"), SerializeField] private SpriteRenderer _sprite;
        [SerializeField] private Collider2D _collider;
        [SerializeField] private int curTeamIndex;
        [SerializeField] private float targetSpeed;

        [Header("Photon"), SerializeField] private Vector2 networkPosition;
        [SerializeField] private float networkLag;

        private Rigidbody2D _rigidbody;
        private PhotonView _photonView;

        // Configurable settings
        private GameVariables variables;

        private void Awake()
        {
            Debug.Log("Awake");
            variables = RuntimeGameConfig.Get().variables;
            _rigidbody = GetComponent<Rigidbody2D>();
            _photonView = PhotonView.Get(this);
            _rigidbody.isKinematic = !_photonView.IsMine;
            _collider.enabled = _photonView.IsMine;

            // Ball starts towards team 1 when started - if nothing is changed externally.
            curTeamIndex = 0;
            targetSpeed = 0;

            enabled = false; // Wait until game starts
        }

        private void OnEnable()
        {
            Debug.Log("OnEnable");
            ((IBallControl)this).showBall();
        }

        private void OnDisable()
        {
            Debug.Log("OnDisable");
            ((IBallControl)this).hideBall();
        }

        void IBallControl.teleportBall(Vector2 position)
        {
            _rigidbody.position = position;
        }

        void IBallControl.moveBall(Vector2 direction, float speed)
        {
            targetSpeed = speed;
            _rigidbody.velocity = direction * speed;
        }

        void IBallControl.showBall()
        {
            _sprite.enabled = true;
            _collider.enabled = true;
        }

        void IBallControl.hideBall()
        {
            _sprite.enabled = false;
            _collider.enabled = false;
            targetSpeed = 0;
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
                randomReset(curTeamIndex);
                return;
            }
            _rigidbody.velocity = Vector2.Lerp(_velocity, targetVelocity, deltaTime * variables.ballLerpSmoothingFactor);
        }

        private void randomReset(int forTeam)
        {
            transform.position = Vector3.zero;
            var direction = forTeam == 0 ? Vector2.up : Vector2.down;
            _rigidbody.velocity = direction * targetSpeed;
        }
    }
}