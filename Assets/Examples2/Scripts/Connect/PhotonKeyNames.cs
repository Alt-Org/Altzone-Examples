using System.Linq;
using Photon.Realtime;
using UnityEngine.Assertions;

namespace Examples2.Scripts.Connect
{
    public static class PhotonKeyNames
    {
        public const string PlayerPosition1 = "p1";
        public const string PlayerPosition2 = "p2";
        public const string PlayerPosition3 = "p3";
        public const string PlayerPosition4 = "p4";

        public const string PlayerPosition = "p";

        public static string GetPlayerPositionKey(int playerPos)
        {
            Assert.IsTrue(playerPos >= 1 && playerPos <= 4, "playerPos >= 1 && playerPos <= 4");
            return $"p{playerPos}";
        }

        public static int GetFreePlayerPosition(this Room room)
        {
            var posKeys = new[] { PlayerPosition1, PlayerPosition2, PlayerPosition1, PlayerPosition2 };
            foreach (var entry in room.CustomProperties)
            {
                if (!posKeys.Contains(entry.Key))
                {
                    continue;
                }
                if ((byte)entry.Value != 0)
                {
                    continue;
                }
                var value = int.Parse(entry.Key.ToString().Substring(1));
                return value;
            }
            return 0;
        }
    }
}