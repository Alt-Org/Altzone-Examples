using Examples2.Scripts.Battle.interfaces;
using Prg.Scripts.Common.Photon;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

namespace Examples2.Scripts.Battle.Room
{
    /// <summary>
    /// Brick manager for the room that synchronizes brick state over network.
    /// </summary>
    public class BrickManager : MonoBehaviour, IBrickManager
    {
        private const int msgDeleteBrick = PhotonEventDispatcher.eventCodeBase + 0;

        [Header("Settings"), SerializeField] private GameObject upperBricks;
        [SerializeField] private GameObject lowerBricks;

        private readonly Dictionary<int, IdMarker> bricks = new Dictionary<int, IdMarker>();

        private PhotonEventDispatcher photonEventDispatcher;

        private void Awake()
        {
            Debug.Log("Awake");
            createBrickMarkersFor(upperBricks.transform, bricks);
            createBrickMarkersFor(lowerBricks.transform, bricks);
            photonEventDispatcher = PhotonEventDispatcher.Get();
            photonEventDispatcher.registerEventListener(msgDeleteBrick, data => { onDeleteBrick(data.CustomData); });
        }

        #region Photon Events

        private void onDeleteBrick(object data)
        {
            var payload = (byte[])data;
            Assert.AreEqual((byte)msgDeleteBrick, payload[0], "Invalid message id");
            var brickId = (int)payload[1];
            if (bricks.TryGetValue(brickId, out var marker))
            {
                bricks.Remove(brickId);
                destroyBrick(marker);
            }
        }

        void IBrickManager.deleteBrick(GameObject brick)
        {
            var brickId = brick.GetComponent<IdMarker>().Id;
            var payload = new[] { (byte)msgDeleteBrick, (byte)brickId };
            photonEventDispatcher.RaiseEvent(msgDeleteBrick, payload);
        }

        #endregion

        #region Brick Management

        private static void createBrickMarkersFor(Transform parentTransform, Dictionary<int, IdMarker> bricks)
        {
            var childCount = parentTransform.childCount;
            for (var i = 0; i < childCount; ++i)
            {
                var child = parentTransform.GetChild(i).gameObject;
                var marker = child.AddComponent<IdMarker>();
                bricks.Add(marker.Id, marker);
            }
        }

        private static void destroyBrick(IdMarker marker)
        {
            Debug.Log($"destroyBrick {marker}");
            var gameObject = marker.gameObject;
            Assert.IsTrue(gameObject.activeSelf, "GameObject is not active for destroy");
            gameObject.SetActive(false);
        }

        #endregion
    }
}