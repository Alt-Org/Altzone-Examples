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
        private const string ServerUrl = "http://localhost:8090/server/";

        [Test, Description("Test that server responds something based on our request")]
        public async Task ThisIsVeryFirstTest()
        {
            // http://localhost:8090/server/move?player1=123&player2=456&item=789

            Debug.Log("test");

            const int playerId1 = 10;
            const int playerId2 = 20;
            var url = GetUrl($"move?player1={playerId1}&player2={playerId2}&item=123");

            var result = await RestApiServiceAsync.ExecuteRequest("GET", url);

            Debug.Log($"result: {result.ToString().Replace('\r', '.').Replace('\n', '.')}");
            Assert.IsTrue(result.Success);
            var data = Json.Deserialize(result.Payload) as Dictionary<string, object>;
            Assert.IsNotNull(data);
            var players = data["players"] as List<object>;
            Assert.IsNotNull(players);
            var playerList = players.Cast<Dictionary<string, object>>().ToList();
            Assert.IsNotNull(playerList);
            Assert.AreEqual(2, playerList.Count);
            
            // Find player objects by ID from returned player list.
            var player1 = playerList.Find(x => x["player_id"].Equals((long)playerId1));
            Assert.IsNotNull(player1);
            var player2 = playerList.Find(x => x["player_id"].Equals((long)playerId2));
            Assert.IsNotNull(player2);

            Debug.Log($"done");
        }

        private static string GetUrl(string path)
        {
            var start = ServerUrl.EndsWith("/") ? ServerUrl.Substring(0, ServerUrl.Length - 1) : ServerUrl;
            var end = path.StartsWith("/") ? path.Substring(1) : path;
            return $"{start}/{end}";
        }
    }
}