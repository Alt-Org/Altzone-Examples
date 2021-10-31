﻿using Prg.Scripts.Common.Unity;
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

        [Header("Tags"), TagSelector] public string redTeamTag;
        [TagSelector] public string blueTeamTag;
    }

    internal class Ball : MonoBehaviour
    {
        [SerializeField] private BallSettings settings;

        [Header("Live Data"), SerializeField] private bool isRedTeamActive;
        [SerializeField] private bool isBlueTeamActive;

        private Rigidbody2D _rigidbody;
        public Collider2D _collider;
        public ContactFilter2D contactFilter;

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
            _collider = GetComponent<Collider2D>();
            contactFilter = new ContactFilter2D
            {
                useTriggers = true,
                useLayerMask = true,
                layerMask = settings.wallMask,
            };
        }

        private void OnEnable()
        {
            _rigidbody.velocity = settings.initialVelocity;
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
                Debug.Log($"ignore {other.name} layer {other.gameObject.layer}");
                return;
            }
            var colliderMask = 1 << layer;
            if (bounceMaskValue == (bounceMaskValue | colliderMask))
            {
                bounce(other);
                return;
            }
            if (teamAreaMaskValue == (teamAreaMaskValue | colliderMask))
            {
                teamEnter(otherGameObject);
                return;
            }
            if (brickMaskValue == (brickMaskValue | colliderMask))
            {
                bounce(other);
                brick(otherGameObject);
                return;
            }
            Debug.Log($"UNHANDLED hit {other.name} layer {layer}");
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
                return;
            }
            var colliderMask = 1 << layer;
            if (teamAreaMaskValue == (teamAreaMaskValue | colliderMask))
            {
                teamExit(otherGameObject);
            }
        }

        public int frameCount;
        public int overlappingCount;
        public Collider2D[] overlappingColliders = new Collider2D[4];
        public bool hasIgnoreCollider;
        public Collider2D ignoreCollider;

        private void bounce(Collider2D other)
        {
            overlappingCount = _rigidbody.OverlapCollider(contactFilter, overlappingColliders);
            if (overlappingCount > 1)
            {
                for (int i = 0; i < overlappingColliders.Length; i++)
                {
                    if (i < overlappingCount)
                    {
                        var oCollider = overlappingColliders[i];
                        Debug.Log($"overlapping {i} {oCollider.name}");
                        if (oCollider.name.EndsWith("Diamond"))
                        {
                            ignoreCollider = oCollider;
                            hasIgnoreCollider = true;
                        }
                    }
                    else
                    {
                        overlappingColliders[i] = null;
                    }
                }
            }
            if (hasIgnoreCollider && other.Equals(ignoreCollider))
            {
                Debug.Log($"SKIP bounce {other.name} {_rigidbody.velocity} frame {frameCount} overlappingCount {overlappingCount}");
                hasIgnoreCollider = false;
                ignoreCollider = null;
                return;
            }
            var currentVelocity = _rigidbody.velocity;
            var position = _rigidbody.position;
            var closestPoint = other.ClosestPoint(position);
            var direction = closestPoint - position;
            reflect(currentVelocity, direction.normalized);
            frameCount = Time.frameCount;
            Debug.Log($"bounce {other.name} @ {position} dir {currentVelocity} <- {_rigidbody.velocity} frame {frameCount} overlappingCount {overlappingCount}");
        }

        private void brick(GameObject brick)
        {
            Debug.Log($"Destroy {brick.name}");
            Destroy(brick);
        }

        private void teamEnter(GameObject teamArea)
        {
            if (teamArea.CompareTag(settings.redTeamTag))
            {
                if (!isRedTeamActive)
                {
                    Debug.Log("isRedTeamActive <- false");
                }
                isRedTeamActive = false;
                return;
            }
            if (teamArea.CompareTag(settings.blueTeamTag))
            {
                if (!isBlueTeamActive)
                {
                    Debug.Log("isBlueTeamActive <- false");
                }
                isBlueTeamActive = false;
            }
        }

        private void teamExit(GameObject teamArea)
        {
            if (teamArea.CompareTag(settings.redTeamTag))
            {
                if (isRedTeamActive)
                {
                    Debug.Log("isRedTeamActive <- true");
                }
                isRedTeamActive = true;
                return;
            }
            if (teamArea.CompareTag(settings.blueTeamTag))
            {
                if (isBlueTeamActive)
                {
                    Debug.Log("isBlueTeamActive <- true");
                }
                isBlueTeamActive = true;
            }
        }

        private void reflect(Vector2 currentVelocity, Vector2 collisionNormal)
        {
            var speed = currentVelocity.magnitude;
            var direction = Vector2.Reflect(currentVelocity.normalized, collisionNormal);
            _rigidbody.velocity = direction * Mathf.Max(speed, settings.minVelocity);
        }
    }
}