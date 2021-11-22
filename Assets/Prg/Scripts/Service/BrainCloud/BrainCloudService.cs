using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using BrainCloud;
using BrainCloud.JsonFx.Json;
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

        private void Awake()
        {
            Assert.IsTrue(_instance == null, "_instance == null");
            _instance = this;
            StartCoroutine(Startup());
        }

        private IEnumerator Startup()
        {
            Debug.Log("Startup");
            yield return null;
            DontDestroyOnLoad(gameObject);
            _brainCloudWrapper = gameObject.AddComponent<BrainCloudWrapper>();
            yield return null;
            Init();
            yield return null;
            Authenticate();
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

        /// <summary>
        /// Authenticates default user on this device.
        /// </summary>
        /// <remarks>
        /// Will create a new Universal user if none exists!
        /// </remarks>
        public static void Authenticate()
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
                // text format is: "(guid)(reversed_guid)"
                var client = Get()._brainCloudWrapper.Client;
                var guid = client.AuthenticationService.GenerateAnonymousId();
                playerName = $"({guid})({Reverse(guid)})";
                playerName = StringSerializer.Encode(playerName);
                PlayerPrefs.SetString(PlayerPrefsPlayerNameKey, playerName);
            }
            playerName = StringSerializer.Decode(playerName);
            var tokens = playerName.Split(new char[] { '(', ')' }, StringSplitOptions.RemoveEmptyEntries);
            Assert.IsTrue(tokens.Length == 2, "tokens.Length == 2");
            Authenticate(tokens[0], tokens[1]);
        }

        public static void Authenticate(string userId, string password)
        {
            string GetPlayerName(string jsonData)
            {
                // Can not use UNITY JsonUtility.FromJson to get Dictionary :-(
                var json = JsonReader.Deserialize<Dictionary<string, object>>(jsonData);
                if (!json.TryGetValue("data", out var @object))
                {
                    return string.Empty;
                }
                if (!(@object is Dictionary<string, object> data))
                {
                    return string.Empty;
                }
                if (data.TryGetValue("playerName", out var playerName))
                {
                    return playerName == null ? string.Empty : playerName.ToString();
                }
                return string.Empty;
            }

            Debug.Log($"Authenticate '{userId}'");
            Get()._brainCloudWrapper.AuthenticateUniversal(userId, password, true,
                (jsonData, ctx) =>
                {
                    var playerName = GetPlayerName(jsonData);
                    Debug.Log($"Authenticate '{userId}' : '{playerName}' OK {jsonData}");
                    if (string.IsNullOrEmpty(playerName))
                    {
                        Debug.Log("NO PLAYER NAME");
                    }
                },
                (status,code,error,ctx)=> {
                    if (code == ReasonCodes.TOKEN_DOES_NOT_MATCH_USER)
                    {
                        Debug.Log($"Authenticate '{userId}' INCORRECT PASSWORD {status} : {code} {error}");
                    }
                    else
                    {
                        Debug.Log($"Authenticate '{userId}' FAILED {status} : {code} {error}");
                    }
                });
        }
    }
}