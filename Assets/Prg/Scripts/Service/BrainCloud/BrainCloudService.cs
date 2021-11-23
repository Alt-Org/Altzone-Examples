using System;
using System.Threading.Tasks;
using BrainCloud.Entity;
using Prg.Scripts.Common.PubSub;
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
        [SerializeField] private BrainCloudUser _brainCloudUser;

        public static bool IsReady => _instance._brainCloudUser != null;

        public static BrainCloudUser BrainCloudUser
        {
            get => _instance._brainCloudUser;
            private set
            {
                _instance._brainCloudUser = value;
                _instance.Publish(_instance._brainCloudUser);
            }
        }

        private async void Awake()
        {
            Debug.Log("Awake");
            Assert.IsTrue(_instance == null, "_instance == null");
            _instance = this;
            DontDestroyOnLoad(gameObject);
            _brainCloudWrapper = gameObject.AddComponent<BrainCloudWrapper>();
            BrainCloudAsync.SetBrainCloudWrapper(_brainCloudWrapper);
            Init(GetAppParams());
            var (userId, password) = GetCredentials();
            var success = Authenticate(userId, password);
            Debug.Log($"brainCloudUser '{_brainCloudUser.UserName}' success {success}");
        }

        /// <summary>
        /// Initializes BrainCLoud.<br />
        /// See: https://getbraincloud.com/apidocs/tutorials/c-sharp-tutorials/getting-started-with-c-sharp/
        /// </summary>
        private void Init(string[] @params)
        {
            Debug.Log("Init");
            var i = -1;
            var url = @params[++i];
            var secretKey = @params[++i];
            var appId = @params[++i];
            var version = @params[++i];
            _brainCloudWrapper.Init(url, secretKey, appId, version);
            // Compress messages larger than 50Kb (default value).
            var client = _brainCloudWrapper.Client;
            client.EnableCompressedRequests(true);
            client.EnableCompressedResponses(true);
        }

        /// <summary>
        /// Authenticates a user using universal authentication.
        /// </summary>
        /// <remarks>
        /// Will create a new user if none exists!
        /// </remarks>
        private static async Task<bool> Authenticate(string userId, string password)
        {
            Assert.IsTrue(!string.IsNullOrWhiteSpace(userId), "!string.IsNullOrWhiteSpace(userId)");
            Assert.IsTrue(!string.IsNullOrWhiteSpace(password), "!string.IsNullOrWhiteSpace(password)");
            BrainCloudUser = await BrainCloudAsync.Authenticate(userId, password);
            if (!BrainCloudUser.IsValid)
            {
                Debug.Log($"Authenticate failed for user {BrainCloudUser.UserId}: {BrainCloudUser.StatusCode}");
            }
            return BrainCloudUser.IsValid;
        }

        public static async Task<bool> UpdateUserName(string playerName)
        {
            Assert.IsTrue(!string.IsNullOrWhiteSpace(playerName), "!string.IsNullOrWhiteSpace(playerName)");
            var result = await BrainCloudAsync.UpdateUserName(playerName);
            if (result == 0)
            {
                var old = _instance._brainCloudUser;
                BrainCloudUser = new BrainCloudUser(old.UserId, playerName, old.ProfileId, 0);
            }
            return result == 0;
        }

        /// <summary>
        /// Gets BrainCLoud init params: url, secretKey, appId and version.
        /// </summary>
        private static string[] GetAppParams()
        {
            // AFAIK Server URL is not publicly available because they want to obscure it by themselves
            // - BrainCloudClient.s_defaultServerURL or BrainCloud.Plugin.Interface.DispatcherURL
            return new[]
            {
                "https://sharedprod.braincloudservers.com/dispatcherv2",
                "11879aa7-33a2-4423-9f2a-21c4b2218844",
                "11589",
                "1.0.0"
            };

        }
        /// <summary>
        /// Gets credentials for BrainCLoud universal login: username and password.
        /// </summary>
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