using System;
using BrainCloud.Entity;
using Prg.Scripts.Common.Util;
using UnityEngine;
using UnityEngine.Assertions;

namespace Prg.Scripts.Service.BrainCloud
{
    public class BrainCloudService : MonoBehaviour
    {
        private const string PlayerPrefsPlayerNameKey = "My.BrainCloud.PlayerName";

        private static BrainCloudService _instance;

        private static BrainCloudService Get() => _instance;

        [SerializeField] private BrainCloudWrapper _brainCloudWrapper;

        private async void Awake()
        {
            Debug.Log("Awake");
            Assert.IsTrue(_instance == null, "_instance == null");
            _instance = this;
            DontDestroyOnLoad(gameObject);
            _brainCloudWrapper = gameObject.AddComponent<BrainCloudWrapper>();
            BrainCloudAsync.SetBrainCloudWrapper(_brainCloudWrapper);
            Init();
            var (userId, password) = GetCredentials();
            var brainCloudUser = await BrainCloudAsync.Authenticate(userId, password);
            if (!brainCloudUser.IsValid)
            {
                Debug.Log($"Authenticate failed for user {brainCloudUser.userId}: {brainCloudUser.statusCode}");
                return;
            }
            Debug.Log($"brainCloudUser '{brainCloudUser.userName}' OK");
        }

        /// <summary>
        /// Initializes BrainCLoud.<br />
        /// See: https://getbraincloud.com/apidocs/tutorials/c-sharp-tutorials/getting-started-with-c-sharp/
        /// </summary>
        private void Init()
        {
            Debug.Log("Init");
            string url = "https://sharedprod.braincloudservers.com/dispatcherv2";
            string secretKey = "11879aa7-33a2-4423-9f2a-21c4b2218844";
            string appId = "11589";
            string version = "1.0.0";
            _brainCloudWrapper.Init(url, secretKey, appId, version);
            // Compress messages larger than 50Kb (default value).
            var client = _brainCloudWrapper.Client;
            client.EnableCompressedRequests(true);
            client.EnableCompressedResponses(true);
        }

        private static Tuple<string, string> GetCredentials()
        {
            string Reverse(string str)
            {
                var chars = str.ToCharArray();
                Array.Reverse(chars);
                return new string(chars);
            }

            var playerName = PlayerPrefs.GetString(PlayerPrefsPlayerNameKey, string.Empty);
            if (string.IsNullOrWhiteSpace(playerName))
            {
                // Text format is: "(guid)(reversed_guid)" for new username and password.
                var client = Get()._brainCloudWrapper.Client;
                var guid = client.AuthenticationService.GenerateAnonymousId();
                playerName = $"({guid})({Reverse(guid)})";
                playerName = StringSerializer.Encode(playerName);
                PlayerPrefs.SetString(PlayerPrefsPlayerNameKey, playerName);
            }
            playerName = StringSerializer.Decode(playerName);
            var tokens = playerName.Split(new[] { '(', ')' }, StringSplitOptions.RemoveEmptyEntries);
            Assert.IsTrue(tokens.Length == 2, "tokens.Length == 2");
            return new Tuple<string, string>(tokens[0], tokens[1]);
        }
   }
}