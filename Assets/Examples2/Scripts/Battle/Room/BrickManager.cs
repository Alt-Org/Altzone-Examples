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
        private const int MsgDeleteBrick = PhotonEventDispatcher.eventCodeBase + 0;

        [Header("Settings"), SerializeField] private GameObject upperBricks;
        [SerializeField] private GameObject lowerBricks;

        private readonly Dictionary<int, IdMarker> _bricks = new Dictionary<int, IdMarker>();

        private PhotonEventDispatcher _photonEventDispatcher;

        private void Awake()
        {
            Debug.Log("Awake");
            CreateBrickMarkersFrom(upperBricks.transform, _bricks);
            CreateBrickMarkersFrom(lowerBricks.transform, _bricks);
            _photonEventDispatcher = PhotonEventDispatcher.Get();
            _photonEventDispatcher.registerEventListener(MsgDeleteBrick, data => { ONDeleteBrick(data.CustomData); });
        }

        #region Photon Events

        private void ONDeleteBrick(object data)
        {
            var payload = (byte[])data;
            Assert.AreEqual((byte)MsgDeleteBrick, payload[0], "Invalid message id");
            var brickId = (int)payload[1];
            if (_bricks.TryGetValue(brickId, out var marker))
            {
                _bricks.Remove(brickId);
                DestroyBrick(marker);
            }
        }

        void IBrickManager.DeleteBrick(GameObject brick)
        {
            var brickId = brick.GetComponent<IdMarker>().Id;
            var payload = new[] { (byte)MsgDeleteBrick, (byte)brickId };
            _photonEventDispatcher.RaiseEvent(MsgDeleteBrick, payload);
        }

        #endregion

        #region Brick Management

        private static void CreateBrickMarkersFrom(Transform parentTransform, IDictionary<int, IdMarker> bricks)
        {
            var childCount = parentTransform.childCount;
            for (var i = 0; i < childCount; ++i)
            {
                var child = parentTransform.GetChild(i).gameObject;
                var marker = child.AddComponent<IdMarker>();
                bricks.Add(marker.Id, marker);
            }
        }

        private static void DestroyBrick(IdMarker marker)
        {
            Debug.Log($"destroyBrick #{marker.Id} {marker.name}");
            var gameObject = marker.gameObject;
            Assert.IsTrue(gameObject.activeSelf, "GameObject is not active for destroy");
            gameObject.SetActive(false);
        }

        #endregion
    }
}