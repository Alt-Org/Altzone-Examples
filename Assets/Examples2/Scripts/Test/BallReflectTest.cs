﻿using Prg.Scripts.Common.Unity;
using System;
using UnityEngine;

namespace Examples2.Scripts.Test
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

    internal class BallReflectTest : MonoBehaviour
    {
        [SerializeField] private BallSettings settings;

        [Header("Live Data"), SerializeField] private bool isRedTeamActive;
        [SerializeField] private bool isBlueTeamActive;

        private Rigidbody2D _rigidbody;

        private int bounceMaskValue;
        private int teamAreaMaskValue;
        private int headMaskValue;
        private int brickMaskValue;
        private int wallMaskValue;

        [Header("Time.timeScale")] public float timeScale;

        [Header("Collider Debug")] public int ignoredCount;
        public Collider2D[] ignoredColliders = new Collider2D[4];
        public ContactFilter2D contactFilter;
        private int overlappingCount;
        private readonly Collider2D[] overlappingColliders = new Collider2D[4];
        private readonly float[] overlappingDistance = new float[4];

        // Diamond hack
        private GameObject TopDiamond;
        private GameObject BottomDiamond;
        private GameObject UpperStoneWall;
        private GameObject LowerStoneWall;
        [Header("Diamond Debug")] public int UpperStoneWallCount;
        public int LowerStoneWallCount;

        private void Awake()
        {
            bounceMaskValue = settings.bounceMask.value;
            teamAreaMaskValue = settings.teamAreaMask.value;
            headMaskValue = settings.headMask.value;
            brickMaskValue = settings.brickMask.value;
            wallMaskValue = settings.wallMask.value;
            _rigidbody = GetComponent<Rigidbody2D>();
            // We need to track these colliders while ball bounces
            contactFilter = new ContactFilter2D
            {
                useTriggers = true,
                useLayerMask = true,
                layerMask = settings.wallMask.value + settings.brickMask.value, // Implicitly converts an integer to a LayerMask
            };
            TopDiamond = GameObject.Find(nameof(TopDiamond));
            BottomDiamond = GameObject.Find(nameof(BottomDiamond));
            UpperStoneWall = GameObject.Find(nameof(UpperStoneWall));
            LowerStoneWall = GameObject.Find(nameof(LowerStoneWall));
            UpperStoneWallCount = UpperStoneWall.transform.childCount;
            LowerStoneWallCount = LowerStoneWall.transform.childCount;
            if (UpperStoneWallCount > 0)
            {
                TopDiamond.SetActive(false);
            }
            if (LowerStoneWallCount > 0)
            {
                BottomDiamond.SetActive(false);
            }
        }

        private void OnEnable()
        {
            _rigidbody.velocity = settings.initialVelocity;
            if (timeScale > 1f)
            {
                Time.timeScale = timeScale;
                Debug.Log($"SET Time.timeScale {Time.timeScale:F3}");
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

        private void OnTriggerStay2D(Collider2D other)
        {
            if (!enabled)
            {
                return; // Collision events will be sent to disabled MonoBehaviours, to allow enabling Behaviours in response to collisions.
            }
            if (ignoredCount > 0)
            {
                for (var i = 0; i < ignoredCount; ++i)
                {
                    if (ignoredColliders[i].Equals(other))
                    {
                        return;
                    }
                }
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
                return;
            }

            Debug.Log($"STOP @ {_rigidbody.position} on STAY hit {other.name} frame {Time.frameCount}");
            _rigidbody.velocity = Vector2.zero;
        }

        private void OnTriggerExit2D(Collider2D other)
        {
            if (!enabled)
            {
                return; // Collision events will be sent to disabled MonoBehaviours, to allow enabling Behaviours in response to collisions.
            }
            if (ignoredCount > 0)
            {
                if (removeIgnoredCollider(other))
                {
                    return;
                }
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

        private void addIgnoredCollider(Collider2D other)
        {
            ignoredColliders[ignoredCount] = other;
            ignoredCount += 1;
        }

        private bool removeIgnoredCollider(Collider2D other)
        {
            for (var i = 0; i < ignoredCount; ++i)
            {
                if (ignoredColliders[i].Equals(other))
                {
                    Debug.Log($"REMOVE ignore {other.name} frame {Time.frameCount} ignored {ignoredCount}");
                    if (ignoredCount == 1)
                    {
                        ignoredColliders[i] = null;
                        ignoredCount = 0;
                        return true;
                    }
                    // Move items down by one
                    Array.Copy(ignoredColliders, i + 1, ignoredColliders, i, ignoredColliders.Length - 2);
                    ignoredColliders[ignoredCount] = null;
                    ignoredCount -= 1;
                    return true;
                }
            }
            return false;
        }

        private void bounce(Collider2D other)
        {
            if (ignoredCount > 0)
            {
                for (var i = 0; i < ignoredCount; ++i)
                {
                    if (ignoredColliders[i].Equals(other))
                    {
                        Debug.Log($"SKIP ignore {other.name} frame {Time.frameCount} ignored {ignoredCount}");
                        return;
                    }
                }
            }
            overlappingCount = _rigidbody.OverlapCollider(contactFilter, overlappingColliders);
            if (overlappingCount < 2)
            {
                bounceAndReflect(other);
                return;
            }
            // Count wall colliders and print print all colliders
            var wallColliderCount = 0;
            var position = _rigidbody.position;
            for (var i = 0; i < overlappingColliders.Length; i++)
            {
                if (i < overlappingCount)
                {
                    var overlappingCollider = overlappingColliders[i];
                    var closest = overlappingCollider.ClosestPoint(_rigidbody.position);
                    overlappingDistance[i] = (closest - position).sqrMagnitude;
                    if (overlappingCollider.name.EndsWith("Wall"))
                    {
                        wallColliderCount += 1;
                    }
                    Debug.Log(
                        $"overlapping {other.name} {i}/{overlappingCount} {overlappingCollider.name} pos {closest} dist {Mathf.Sqrt(overlappingDistance[i]):F3}");
                }
                else
                {
                    overlappingColliders[i] = null;
                }
            }
            if (wallColliderCount == overlappingCount)
            {
                // Let wall colliders run normally
                bounceAndReflect(other);
                return;
            }
            // Collide with nearest only
            var nearest = 0;
            for (var i = 1; i < overlappingCount; i++)
            {
                if (overlappingDistance[i] < overlappingDistance[nearest])
                {
                    nearest = i;
                }
            }
            // Add everything to ignored colliders so that ball can move out while bouncing
            ignoredCount = 0;
            for (var i = 0; i < overlappingCount; i++)
            {
                addIgnoredCollider(overlappingColliders[i]);
            }
            // Do the bounce
            var nearestCollider = overlappingColliders[nearest];
            bounceAndReflect(nearestCollider);
        }

        private void brick(GameObject brick)
        {
            Debug.Log($"Destroy {brick.name} brick count {UpperStoneWallCount + LowerStoneWallCount}");
            Destroy(brick);
            if (ignoredCount > 0)
            {
                var brickCollider = brick.GetComponent<Collider2D>();
                removeIgnoredCollider(brickCollider);
            }
            // Diamond hack
            brick.transform.SetParent(null);
            if (UpperStoneWallCount > 0)
            {
                UpperStoneWallCount = UpperStoneWall.transform.childCount;
                if (UpperStoneWallCount == 0)
                {
                    TopDiamond.SetActive(true);
                    Debug.Log($"SetActive {TopDiamond.name}");
                    var diamondCollider = TopDiamond.GetComponent<Collider2D>();
                    addIgnoredCollider(diamondCollider);
                }
            }
            if (LowerStoneWallCount > 0)
            {
                LowerStoneWallCount = LowerStoneWall.transform.childCount;
                if (LowerStoneWallCount == 0)
                {
                    BottomDiamond.SetActive(true);
                    Debug.Log($"SetActive {BottomDiamond.name}");
                    var diamondCollider = BottomDiamond.GetComponent<Collider2D>();
                    addIgnoredCollider(diamondCollider);
                }
            }
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

        private void bounceAndReflect(Collider2D other)
        {
            var currentVelocity = _rigidbody.velocity;
            var position = _rigidbody.position;
            var closestPoint = other.ClosestPoint(position);
            var direction = closestPoint - position;
            reflect(currentVelocity, direction.normalized);
            Debug.Log(
                $"bounce {other.name} @ {position} closest {closestPoint} dir {currentVelocity} <- {_rigidbody.velocity} frame {Time.frameCount} ol-count {overlappingCount}");
        }

        private void reflect(Vector2 currentVelocity, Vector2 collisionNormal)
        {
            var speed = currentVelocity.magnitude;
            var direction = Vector2.Reflect(currentVelocity.normalized, collisionNormal);
            _rigidbody.velocity = direction * Mathf.Max(speed, settings.minVelocity);
        }
    }
}