using System.Collections;
using Photon.Pun;
using Prg.Scripts.Common.Photon;
using UnityEngine;
using UnityEngine.Assertions;

namespace Examples2.Scripts.Battle.Factory
{
    /// <summary>
    /// Activates networked components when all participants have been created, on for each player.
    /// </summary>
    public class NetworkSync : MonoBehaviour
    {
        private const int MsgNetworkCreated = PhotonEventDispatcher.eventCodeBase + 1;
        private const int MsgNetworkReady = PhotonEventDispatcher.eventCodeBase + 2;

        [Header("Settings"), Min(1), SerializeField] private int _componentTypeId;
        [SerializeField] private MonoBehaviour[] _componentsToActivate;

        [Header("Live Data"), SerializeField] private int _requiredComponentCount;
        [SerializeField] private int _currentComponentCount;

        private PhotonEventDispatcher _photonEventDispatcher;

        private void Awake()
        {
            if (PhotonNetwork.OfflineMode)
            {
                ActivateAllComponents();
                enabled = false;
                return;
            }
            _requiredComponentCount = PhotonNetwork.CurrentRoom.PlayerCount;
            Debug.Log($"Awake required {_requiredComponentCount} type {_componentTypeId}");
            _photonEventDispatcher = PhotonEventDispatcher.Get();
            _photonEventDispatcher.registerEventListener(MsgNetworkCreated, data => { OnNetworkCreated(data.CustomData); });
            _photonEventDispatcher.registerEventListener(MsgNetworkReady, data => { OnMsgNetworkReady(data.CustomData); });
        }

        private void OnEnable()
        {
            SendNetworkCreated(_componentTypeId);
        }

        #region Photon Events

        private void SendNetworkCreated(int componentTypeId)
        {
            var payload = new object[] { (byte)MsgNetworkCreated, componentTypeId };
            _photonEventDispatcher.RaiseEvent(MsgNetworkCreated, payload);
        }

        private void OnNetworkCreated(object data)
        {
            var payload = (object[])data;
            Assert.AreEqual(payload.Length, 2, "Invalid message length");
            Assert.AreEqual((byte)MsgNetworkCreated, (byte)payload[0], "Invalid message id");
            var componentTypeId = (int)payload[1];
            if (componentTypeId == _componentTypeId)
            {
                _currentComponentCount += 1;
                Debug.Log($"OnNetworkCreated required {_requiredComponentCount} current {_currentComponentCount} type {componentTypeId}");
                if (_currentComponentCount == _requiredComponentCount)
                {
                    if (PhotonNetwork.IsMasterClient)
                    {
                        SendMsgNetworkReady(_componentTypeId);
                    }
                }
            }
        }

        private void SendMsgNetworkReady(int componentTypeId)
        {
            var payload = new object[] { (byte)MsgNetworkCreated, componentTypeId };
            _photonEventDispatcher.RaiseEvent(MsgNetworkReady, payload);
        }

        private void OnMsgNetworkReady(object data)
        {
            var payload = (object[])data;
            Assert.AreEqual(payload.Length, 2, "Invalid message length");
            Assert.AreEqual((byte)MsgNetworkCreated, (byte)payload[0], "Invalid message id");
            var componentTypeId = (int)payload[1];
            if (componentTypeId == _componentTypeId)
            {
                Debug.Log($"OnMsgNetworkReady required {_requiredComponentCount} type {componentTypeId}");
                ActivateAllComponents();
                enabled = false;
            }
        }

        #endregion

        private void ActivateAllComponents()
        {
            Debug.Log($"ActivateAllComponents components {_componentsToActivate.Length} type {_componentTypeId}");
            StartCoroutine(ActivateComponents(_componentsToActivate));
        }

        private static IEnumerator ActivateComponents(MonoBehaviour[] componentsToActivate)
        {
            // Enable one component per frame in array sequence
            for (var i = 0; i < componentsToActivate.LongLength; i++)
            {
                yield return null;
                componentsToActivate[i].enabled = true;
            }
        }
    }
}