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
            this.Unsubscribe();
        }

        private IEnumerator Start()
        {
            yield return new WaitUntil(() => BrainCloudService.IsReady);
            var user = BrainCloudService.BrainCloudUser;
            Debug.Log($"BrainCloudUser {user}");
            enabled = user.IsValid;
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

        private static async void SetPlayerName(string playerName)
        {
            Debug.Log($"SetPlayerName '{playerName}'");
            var result = await BrainCloudService.UpdateUserName(playerName);
            Debug.Log($"UpdateUserName success={result} {BrainCloudService.BrainCloudUser}");
        }
    }
}