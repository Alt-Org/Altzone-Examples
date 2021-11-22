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
        [Header("Debug Only")] public string _playerName;
        public bool _setPlayerName;

        private void Awake()
        {
            this.Subscribe<BrainCloudUser>(OnBrainCloudUser);
        }

        private static void OnBrainCloudUser(BrainCloudUser data)
        {
            Debug.Log($"OnBrainCloudUser {data}");
        }

        private void OnDestroy()
        {
            throw new NotImplementedException();
        }

        private IEnumerator Start()
        {
            yield return new WaitUntil(() => BrainCloudService.IsReady);
            Debug.Log($"BrainCloudUser {BrainCloudService.BrainCloudUser}");
        }

        private void Update()
        {
            if (_setPlayerName)
            {
                _setPlayerName = false;
                Assert.IsTrue(!string.IsNullOrWhiteSpace(_playerName), "!string.IsNullOrWhiteSpace(_playerName)");
                SetPlayerName(_playerName);
            }
        }

        private static void SetPlayerName(string playerName)
        {
            Debug.Log($"SetPlayerName '{playerName}'");
            Debug.Log($"BrainCloudUser {BrainCloudService.BrainCloudUser}");
        }
    }
}