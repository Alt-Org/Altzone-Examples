using System;
using System.Collections;
using BrainCloud;
using UnityEngine;
using UnityEngine.Assertions;

namespace Prg.Scripts.Service.BrainCloud
{
    public class BrainCloudService : MonoBehaviour
    {
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
            string Reverse(string str)
            {
                var chars = str.ToCharArray();
                Array.Reverse(chars);
                return new string(chars);
            }
            Debug.Log("Startup");
            yield return null;
            DontDestroyOnLoad(gameObject);
            _brainCloudWrapper = gameObject.AddComponent<BrainCloudWrapper>();
            yield return null;
            Debug.Log("Init");
            string url = "https://sharedprod.braincloudservers.com/dispatcherv2";
            string secretKey = "11879aa7-33a2-4423-9f2a-21c4b2218844";
            string appId = "11589";
            string version = "1.0.0";
            _brainCloudWrapper.Init(url, secretKey, appId, version);
            yield return null;
            var userId = "teppo";
            Authenticate(userId, Reverse(userId));
        }

        public static void Authenticate(string userId, string password)
        {
            Debug.Log($"Authenticate '{userId}'");
            Get()._brainCloudWrapper.AuthenticateUniversal(userId, password, true,
                (jsonData, ctx) =>
                {
                    Debug.Log($"Authenticate '{userId}' OK {jsonData}");
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