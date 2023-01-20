using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NUnit.Framework;
using Prg.Scripts.Common.Http.RestApi;
using Prg.Scripts.Common.MiniJson;

namespace Tests.EditMode.GameServerTests
{
    /// <summary>
    /// Tests for our own private Game Server API. Remember to start it for these tests!
    /// </summary>
    /// <remarks>
    /// Json.Deserialize returns:<br />
    /// integers as <c>long</c><br />
    /// decimals as <c>double</c><br />
    /// objects as Dictionary&lt;<c>string</c>, <c>object</c>&gt;<br />
    /// lists as List&lt;<c>object</c>&gt;<br />
    /// </remarks>
    [TestFixture]
    public class GameServerTest
    {
        private const int TestServerPort = 8090;
        private const string TestServerAppId = "123456";
        private const string AuthorizationHeaderPrefix = "Bearer ";
        
        private ServerUrl _serverUrl;

        private bool _isAuthenticationRequired;
        private RestApiServiceAsync.AuthorizationHeader _authorizationHeader;

        [OneTimeSetUp]
        public void OneTimeSetUp()
        {
            _serverUrl = new ServerUrl($"http://localhost:{TestServerPort}");
            _isAuthenticationRequired = true;
            Debug.Log($"{_serverUrl} isAuthenticationRequired {_isAuthenticationRequired}");
        }

        [OneTimeTearDown]
        public void OneTimeTearDown()
        {
            Debug.Log("");
        }

        [Test]
        public async Task PingTest()
        {
            var url = _serverUrl.GetUrlFor("test/ping");
            Debug.Log($"test {url}");

            var result = await RestApiServiceAsync.ExecuteRequest("GET", url);

            Debug.Log($"result: {result.ToString().Replace('\r', '.').Replace('\n', '.')}");
            AssertSuccess(result);

            var data = MiniJson.Deserialize(result.Payload) as Dictionary<string, object>;
            Assert.IsNotNull(data);
        }

        [Test]
        public async Task AuthenticationTest()
        {
            // http://localhost:8090/login/authenticate?appid=123456
            
            var url = _serverUrl.GetUrlFor($"login/authenticate?appid={TestServerAppId}");
            Debug.Log($"test {url}");
            
            var result = await RestApiServiceAsync.ExecuteRequest("GET", url);
            Debug.Log($"result: {result.ToString().Replace('\r', '.').Replace('\n', '.')}");
            AssertSuccess(result);


            var data = MiniJson.Deserialize(result.Payload) as Dictionary<string, object>;
            Assert.IsNotNull(data);
            var key = data["AuthenticationKey"] as string;
            Assert.IsNotNull(key);
        }
        
        [Test]
        public async Task ClanListTest()
        {
            // http://localhost:8090/server/clan/list

            var url = _serverUrl.GetUrlFor("server/clan/list");
            Debug.Log($"test {url}");

            if (_isAuthenticationRequired && _authorizationHeader == null)
            {
                await CheckAuthentication();
            }
            //var key = "Abba";
            //_authorizationHeader = new RestApiServiceAsync.AuthorizationHeader($"{AuthorizationHeaderPrefix}{key}");
            var result = await RestApiServiceAsync.ExecuteRequest("GET", url, null, _authorizationHeader);
            Debug.Log($"result: {result.ToString().Replace('\r', '.').Replace('\n', '.')}");
            AssertSuccess(result);

            var data = MiniJson.Deserialize(result.Payload) as Dictionary<string, object>;
            Assert.IsNotNull(data);
            var clans = data["clans"] as List<object>;
            Assert.IsNotNull(clans);
            var clanList = clans.Cast<Dictionary<string, object>>().ToList();
            Assert.IsNotNull(clanList);

            Debug.Log($"done");
        }

        private async Task CheckAuthentication()
        {
            var url = _serverUrl.GetUrlFor($"login/authenticate?appid={TestServerAppId}");
            Debug.Log($"url {url}");

            var result = await RestApiServiceAsync.ExecuteRequest("GET", url);
            Debug.Log($"result: {result.ToString().Replace('\r', '.').Replace('\n', '.')}");
            AssertSuccess(result);

            var data = MiniJson.Deserialize(result.Payload) as Dictionary<string, object>;
            Assert.IsNotNull(data);
            var key = data["AuthenticationKey"] as string;
            Assert.IsNotNull(key);

            Debug.Log($"key {key}");
            _authorizationHeader = new RestApiServiceAsync.AuthorizationHeader($"{AuthorizationHeaderPrefix}{key}");
        }

        private static void AssertSuccess(RestApiServiceAsync.Response response)
        {
            if (!response.Success)
            {
                Debug.Log("*");
                Debug.Log("* CHECK THAT SERVER IS RUNNING");
                Debug.Log("*");
            }
            Assert.IsTrue(response.Success);
        }
    }
}