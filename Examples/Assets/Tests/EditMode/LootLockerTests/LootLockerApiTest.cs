using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using LootLocker;
using NUnit.Framework;
using Prg.Scripts.Common.MiniJson;
using Prg.Scripts.Common.RestApi;

namespace Tests.EditMode.LootLockerTests
{
    [TestFixture]
    public class LootLockerApiTest
    {
        private const string ServerUrl = "https://api.lootlocker.io/server/";
        private const string ServerApiKey = "dev_7c9da977316f449a881f5424215b9902";
        private const string GameVersion = "{\"game_version\": \"0.1.0.0\"}";

        private bool HasSession => _sessionToken != null;

        private string _sessionToken;

        [OneTimeSetUp]
        public void OneTimeSetUp()
        {
#if !USE_LOOTLOCKER
            Assert.IsTrue(false, "USE_LOOTLOCKER is not defined");
#endif
        }

        [Test]
        public async Task ListCharacterTypesTest()
        {
            Debug.Log($"test HasSession {HasSession}");
            if (!HasSession)
            {
                await RegisterServerSession();
            }
            Assert.IsTrue(HasSession);

            Debug.Log($"done");
        }

        private async Task RegisterServerSession()
        {
            _sessionToken = null;
            var url = GetUrl("session");
            const string postData = GameVersion;
            Debug.Log($"postData {postData}");
            var version = LootLockerConfig.Get().dateVersion;
            var headerValues = new List<Tuple<string, string>>
            {
                new(version.key, version.value),
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