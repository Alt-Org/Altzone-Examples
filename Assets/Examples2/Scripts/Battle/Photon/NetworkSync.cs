using System.Collections;
using Examples2.Scripts.Battle.Room;
using Photon.Pun;
using Prg.Scripts.Common.Photon;
using Prg.Scripts.Common.PubSub;
using UnityEngine;
using UnityEngine.Assertions;

namespace Examples2.Scripts.Battle.Photon
{
    /// <summary>
    /// Activates networked components when all participants have been created, on for each player.
    /// </summary>
    [RequireComponent(typeof(PhotonView))]
    internal class NetworkSync : MonoBehaviour
    {
        private const int MsgNetworkCreated = PhotonEventDispatcher.eventCodeBase + 1;
        private const int MsgNetworkReady = PhotonEventDispatcher.eventCodeBase + 2;

        [Header("Settings"), Min(1), SerializeField] private int _componentTypeId;
        [SerializeField] private MonoBehaviour[] _componentsToActivate;

        [Header("Live Data"), SerializeField] private int _requiredComponentCount;
        [SerializeField] private int _currentComponentCount;

        private PhotonView _photonView;
        private PhotonEventDispatcher _photonEventDispatcher;

        private void Awake()
        {
            Assert.IsTrue(_componentsToActivate.Length > 0, "No components to activate");
            foreach (var component in _componentsToActivate)
            {
                Assert.IsFalse(component.enabled, $"Component is active: {component.GetType().Name}");
            }
            if (PhotonNetwork.OfflineMode)
            {
                ActivateAllComponents();
                enabled = false;
                return;
            }
            _requiredComponentCount = PhotonBattle.CountRealPlayers();
            Debug.Log($"Awake {name} required {_requiredComponentCount} type {_componentTypeId}");
            _photonView = PhotonView.Get(this);
            _photonEventDispatcher = PhotonEventDispatcher.Get();
            _photonEventDispatcher.registerEventListener(MsgNetworkCreated, data => { OnNetworkCreated(data.CustomData); });
            _photonEventDispatcher.registerEventListener(MsgNetworkReady, data => { OnMsgNetworkReady(data.CustomData); });
        }

        private void OnEnable()
        {
            if (_photonView.IsMine || _photonView.IsRoomView)
            {
                Debug.Log($"OnEnable {name} required {_requiredComponentCount} type {_componentTypeId}");
                SendNetworkCreated(_componentTypeId);
            }
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
                Debug.Log(
                    $"OnNetworkCreated {name} required {_requiredComponentCount} current {_currentComponentCount} type {componentTypeId} master {PhotonNetwork.IsMasterClient}");
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
                Debug.Log($"OnMsgNetworkReady {name} required {_requiredComponentCount} type {componentTypeId}");
                ActivateAllComponents();
                enabled = false;
            }
        }

        #endregion

        private void ActivateAllComponents()
        {
            Debug.Log($"ActivateAllComponents {name} components {_componentsToActivate.Length} type {_componentTypeId}");
            StartCoroutine(ActivateComponents());
        }

        private IEnumerator ActivateComponents()
        {
            // Enable one component per frame in array sequence
            for (var i = 0; i < _componentsToActivate.LongLength; i++)
            {
                yield return null;
                _componentsToActivate[i].enabled = true;
            }
            yield return null;
            this.Publish(new RoomManager.ActorReportEvent(_componentTypeId));
        }
    }
}