using System;
using System.Collections;
using Prg.Scripts.Common.PubSub;
using Prg.Scripts.Service.BrainCloud;
using UnityEngine;
using UnityEngine.Assertions;

namespace Examples2.Scripts.Test
{
    public class BrainCloudTest : MonoBehaviour
    {
        public string _status;
        [Header("Debug Only")] public string _playerName;
        public bool _setPlayerName;
        public bool _showPlayerOnLog;

        private void Awake()
        {
            _status = "initializing";
            this.Subscribe<BrainCloudUser>(OnBrainCloudUser);
        }

        private void OnBrainCloudUser(BrainCloudUser user)
        {
            _status =  user.IsValid ? "ready" : "failed";
            Debug.Log($"OnBrainCloudUser {user}");
            enabled = user.IsValid;
        }

        private void OnDestroy()
        {
            this.Unsubscribe();
        }

        private IEnumerator Start()
        {
            var service = BrainCloudService.Get();
            var (userId, password) = BrainCloudSupport.GetCredentials();
            service.Authenticate(userId, password);

            yield return new WaitUntil(() => service.IsReady);
            var user = service.BrainCloudUser;
            Debug.Log($"BrainCloudUser {user}");
        }

        private void Update()
        {
            if (_setPlayerName)
            {
                _setPlayerName = false;
                Assert.IsTrue(!string.IsNullOrWhiteSpace(_playerName), "!string.IsNullOrWhiteSpace(_playerName)");
                SetPlayerName(_playerName);
            }
            if (_showPlayerOnLog)
            {
                _showPlayerOnLog = false;
                Debug.Log($"BrainCloudUser is {BrainCloudService.Get().BrainCloudUser}");
            }
        }

        private static async void SetPlayerName(string playerName)
        {
            Debug.Log($"SetPlayerName '{playerName}'");
            var service = BrainCloudService.Get();
            var result = await service.UpdateUserName(playerName);
            Debug.Log($"UpdateUserName success={result} {service.BrainCloudUser}");
        }
    }
}