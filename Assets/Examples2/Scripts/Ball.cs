using UnityEngine;

namespace Examples2.Scripts
{
    public class Ball : MonoBehaviour
    {
        [SerializeField] private Vector2 initialVelocity;
        [SerializeField] private float minVelocity = 3f;

        private Rigidbody2D _rigidbody;
        private Vector2 currentVelocity;

        private void Awake()
        {
            _rigidbody = GetComponent<Rigidbody2D>();
        }

        private void OnEnable()
        {
            _rigidbody.velocity = initialVelocity;
        }

        private void Update()
        {
            currentVelocity = _rigidbody.velocity;
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            Debug.Log($"hit {other.name}");
            var position = _rigidbody.position;
            var closestPoint = other.ClosestPoint(position);
            var direction = closestPoint - position;
            bounceFrom(direction.normalized);
        }

        private void bounceFrom(Vector2 collisionNormal)
        {
            var speed = currentVelocity.magnitude;
            var direction = Vector2.Reflect(currentVelocity.normalized, collisionNormal);
            Debug.Log($"bounce @ {_rigidbody.position} dir {direction}");
            _rigidbody.velocity = direction * Mathf.Max(speed, minVelocity);
        }
    }
}