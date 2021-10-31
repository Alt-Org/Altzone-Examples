using Prg.Scripts.Common.Unity;
using System;
using UnityEngine;

namespace Examples2.Scripts
{
    [Serializable]
    internal class BallSettings
    {
        [Header("Ball movement")] public Vector2 initialVelocity;
        public float minVelocity;

        [Header("Layers")] public LayerMask bounceMask;
        public LayerMask teamAreaMask;
        public LayerMask headMask;
        public LayerMask brickMask;
        public LayerMask wallMask;

        [Header("Tags"), TagSelector] public string readTeamTag;
        [TagSelector] public string blueTeamTag;
    }

    internal class Ball : MonoBehaviour
    {
        [SerializeField] private BallSettings settings;

        private Rigidbody2D _rigidbody;
        private Vector2 currentVelocity;

        private int bounceMaskValue;
        private int teamAreaMaskValue;
        private int headMaskValue;
        private int brickMaskValue;
        private int wallMaskValue;

        private void Awake()
        {
            bounceMaskValue = settings.bounceMask.value;
            teamAreaMaskValue = settings.teamAreaMask.value;
            headMaskValue = settings.headMask.value;
            brickMaskValue = settings.brickMask.value;
            wallMaskValue = settings.wallMask.value;
            _rigidbody = GetComponent<Rigidbody2D>();
        }

        private void OnEnable()
        {
            _rigidbody.velocity = settings.initialVelocity;
        }

        private void Update()
        {
            currentVelocity = _rigidbody.velocity;
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            var otherGameObject = other.gameObject;
            var layer = otherGameObject.layer;
            if (layer == 0)
            {
                Debug.Log($"ignore {other.name} layer {other.gameObject.layer}");
                return;
            }
            var colliderMask = 1 << layer;
            if (bounceMaskValue == (bounceMaskValue | colliderMask))
            {
                var position = _rigidbody.position;
                var closestPoint = other.ClosestPoint(position);
                var direction = closestPoint - position;
                bounceFrom(direction.normalized);
                return;
            }
            Debug.Log($"UNHANDLED hit {other.name} layer {layer}");
        }

        private void bounceFrom(Vector2 collisionNormal)
        {
            var speed = currentVelocity.magnitude;
            var direction = Vector2.Reflect(currentVelocity.normalized, collisionNormal);
            Debug.Log($"bounce @ {_rigidbody.position} dir {direction}");
            _rigidbody.velocity = direction * Mathf.Max(speed, settings.minVelocity);
        }
    }
}