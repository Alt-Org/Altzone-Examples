using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using LootLocker;
using NUnit.Framework;
using Prg.Scripts.Common.MiniJson;
using Prg.Scripts.Common.RestApi;

namespace Tests.EditMode.LootLockerTests
{
    /// <summary>
    /// Tests for <c>LootLocker</c> Server API.
    /// </summary>
    [TestFixture]
    public class LootLockerApiTest
    {
        private const string ServerUrl = "https://api.lootlocker.io/server/";
        private const string ServerApiKey = "dev_7c9da977316f449a881f5424215b9902";
        private const string GameVersion = "{\"game_version\": \"0.1.0.0\"}";

        private static bool HasSession => _sessionToken != null;
        private static string _sessionToken;

        private (string key, string value) _dateVersion;

        [OneTimeSetUp]
        public void OneTimeSetUp()
        {
#if !USE_LOOTLOCKER
            Assert.IsTrue(false, "USE_LOOTLOCKER is not defined");
#endif
            _dateVersion = LootLockerConfig.Get().dateVersion;
        }

        [Test]
        public async Task PlayerPersistentStorageTest()
        {
            // https://ref.lootlocker.com/server-api/#player-persistent-storage
            
            Debug.Log($"test HasSession {HasSession}");
            if (!HasSession)
            {
                await RegisterServerSession();
            }
            Assert.IsTrue(HasSession);

            var clanId1 = 3022592;
            var clanId2 = 3027563;
            var url = GetUrl($"players/storage?player_ids={clanId1},{clanId2}");

            var headers = new RestApiServiceAsync.Headers(new List<Tuple<string, string>>
            {
                new(_dateVersion.key, _dateVersion.value),
                new("x-auth-token", _sessionToken),
            });
            var result = await RestApiServiceAsync.ExecuteRequest("GET", url, null, headers);
            if (!(Json.Deserialize(result.Payload) is Dictionary<string, object> jsonData))
            {
                Debug.Log($"JSON ERROR {result.Payload.Replace('\r', '.').Replace('\n', '.')}");
                Assert.IsTrue(false);
                return;
            }
            Debug.Log($"data {jsonData.Count}");
            Assert.IsTrue(jsonData.ContainsKey("items"));
            Debug.Log($"items:\r\n{Json.Serialize(jsonData["items"])}");
            Debug.Log($"done");
        }
        
        [Test]
        public async Task PlayerNamesTest()
        {
            // https://ref.lootlocker.com/server-api/#player-names
            
            Debug.Log($"test HasSession {HasSession}");
            if (!HasSession)
            {
                await RegisterServerSession();
            }
            Assert.IsTrue(HasSession);

            var clanId1 = 3022592;
            var clanId2 = 3027563;
            var url = GetUrl($"players/lookup/name?player_id={clanId1}&player_id={clanId2}");
            
            var headers = new RestApiServiceAsync.Headers(new List<Tuple<string, string>>
            {
                new(_dateVersion.key, _dateVersion.value),
                new("x-auth-token", _sessionToken),
            });
            var result = await RestApiServiceAsync.ExecuteRequest("GET", url, null, headers);
            if (!(Json.Deserialize(result.Payload) is Dictionary<string, object> jsonData))
            {
                Debug.Log($"JSON ERROR {result.Payload.Replace('\r', '.').Replace('\n', '.')}");
                Assert.IsTrue(false);
                return;
            }
            Debug.Log($"data {jsonData.Count}");
            Assert.IsTrue(jsonData.ContainsKey("players"));
            Debug.Log($"players:\r\n{Json.Serialize(jsonData["players"])}");
            
            Debug.Log($"done");
        }

        [Test]
        public async Task ServerSessionPingTest()
        {
            // https://ref.lootlocker.com/server-api/#registering-a-server-session
            
            Debug.Log($"test HasSession {HasSession}");
            if (!HasSession)
            {
                await RegisterServerSession();
            }
            Assert.IsTrue(HasSession);

            var url = GetUrl("ping");
            
            var headers = new RestApiServiceAsync.Headers(new List<Tuple<string, string>>
            {
                new(_dateVersion.key, _dateVersion.value),
                new("x-auth-token", _sessionToken),
            });
            
            var result = await RestApiServiceAsync.ExecuteRequest("GET", url, null, headers);
            Debug.Log($"Success {result.Success} {result.Payload.Replace('\r', '.').Replace('\n', '.')}");
            Assert.IsTrue(result.Success);
            
            Debug.Log($"done");
        }
        
        private async Task RegisterServerSession()
        {
            _sessionToken = null;
            var url = GetUrl("session");
            const string postData = GameVersion;
            Debug.Log($"postData {postData}");
            var headerValues = new List<Tuple<string, string>>
            {
                new(_dateVersion.key, _dateVersion.value),
                new("x-server-key", ServerApiKey),
            };
            var headers = new RestApiServiceAsync.Headers(headerValues);
            var result = await RestApiServiceAsync.ExecuteRequest("POST", url, postData, headers);
            if (!(Json.Deserialize(result.Payload) is Dictionary<string, object> jsonData))
            {
                Debug.Log($"JSON ERROR {result.Payload.Replace('\r', '.').Replace('\n', '.')}");
                return;
            }
            Debug.Log($"data {jsonData.Count}");
            foreach (var entry in jsonData)
            {
                Debug.Log($"{entry.Key}={entry.Value}");
            }
            if (result.Success && jsonData.TryGetValue("token", out var token))
            {
                _sessionToken = token.ToString();
            }
        }

        private static string GetUrl(string path)
        {
            var start = ServerUrl.EndsWith("/") ? ServerUrl.Substring(0, ServerUrl.Length - 1) : ServerUrl;
            var end = path.StartsWith("/") ? path.Substring(1) : path;
            return $"{start}/{end}";
        }
    }
}