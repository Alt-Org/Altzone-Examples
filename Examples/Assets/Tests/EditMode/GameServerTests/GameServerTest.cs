using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NUnit.Framework;
using Prg.Scripts.Common.MiniJson;
using Prg.Scripts.Common.RestApi;

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
        private ServerUrl _serverUrl;

        [OneTimeSetUp]
        public void OneTimeSetUp()
        {
            _serverUrl = new ServerUrl($"http://localhost:{TestServerPort}/server/");
            Debug.Log($"{_serverUrl}");
        }
        
        [Test]
        public async Task ClanListTest()
        {
            // http://localhost:8090/server/clan/list

            var url = _serverUrl.GetUrlFor("clan/list");
            Debug.Log($"test {url}");

            var result = await RestApiServiceAsync.ExecuteRequest("GET", url);

            Debug.Log($"result: {result.ToString().Replace('\r', '.').Replace('\n', '.')}");
            if (!result.Success)
            {
                Debug.Log("*");
                Debug.Log("* CHECK THAT SERVER IS RUNNING");
                Debug.Log("*");
                Assert.IsTrue(false);
            }
            Assert.IsTrue(result.Success);
            var data = MiniJson.Deserialize(result.Payload) as Dictionary<string, object>;
            Assert.IsNotNull(data);
            var clans = data["clans"] as List<object>;
            Assert.IsNotNull(clans);
            var clanList = clans.Cast<Dictionary<string, object>>().ToList();
            Assert.IsNotNull(clanList);

            Debug.Log($"done");
        }
    }
}