using System;
using System.Collections;
using NUnit.Framework;
using Photon.Pun;
using Photon.Realtime;
using Prg.Scripts.Common.Photon;
using UnityEngine;
using UnityEngine.TestTools;

namespace Tests.PlayMode
{
    public class PhotonOfflineTest
    {
        private const string PlayerName = "Teppo";
        private const string RoomName = "Test1";
        private const float Timeout = 2.0f;

        [OneTimeSetUp]
        public void OneTimeSetUp()
        {
            Debug.Log($"OneTimeSetUp start {PhotonWrapper.NetworkClientState}");
            PhotonNetwork.OfflineMode = true;
            PhotonNetwork.NickName = PlayerName;
            PhotonNetwork.JoinRandomRoom();
            Debug.Log($"OneTimeSetUp exit {PhotonWrapper.NetworkClientState}");
        }

        [OneTimeTearDown]
        public void OneTimeTearDown()
        {
            Debug.Log($"OneTimeTearDown start {PhotonWrapper.NetworkClientState}");
            PhotonNetwork.Disconnect();
            PhotonNetwork.OfflineMode = false;
            Debug.Log($"OneTimeTearDown exit {PhotonWrapper.NetworkClientState}");
        }

        // A UnityTest behaves like a coroutine in Play Mode. In Edit Mode you can use
        // `yield return null;` to skip a frame.
        [UnityTest]
        public IEnumerator ConnectToPhoton()
        {
            Debug.Log($"test start {PhotonWrapper.NetworkClientState}");

            var inRoomTimeout = Time.time + Timeout;
            yield return new WaitUntil(() => TimedWait(() => PhotonWrapper.InRoom, inRoomTimeout));
            Assert.IsTrue(PhotonNetwork.InRoom, "Not in room");

            yield return RoomCustomPropertiesTests(PhotonNetwork.CurrentRoom);
            yield return null;
            yield return PlayerCustomPropertiesTests(PhotonNetwork.LocalPlayer);

            Debug.Log($"test end {PhotonWrapper.NetworkClientState}");
        }

        private static IEnumerator RoomCustomPropertiesTests(Room room)
        {
            Debug.Log($"test {room.GetDebugLabel()}");

            const string intPropName = "INT";
            const string stringPropName = "STR";
            const int intValue = 123;
            const int newIntValue = 321;
            const string stringValue = "123";
            const string newStringValue = "321";

            var hasProp = room.HasCustomProperty(intPropName);
            Assert.IsFalse(hasProp);
            yield return null;

            room.SetCustomProperty(intPropName, intValue);
            yield return null;

            hasProp = room.HasCustomProperty(intPropName);
            Assert.IsTrue(hasProp);
            yield return null;

            room.SetCustomProperty(stringPropName, stringValue);
            yield return null;

            hasProp = room.HasCustomProperty(stringPropName);
            Assert.IsTrue(hasProp);
            yield return null;
        }

        private static IEnumerator PlayerCustomPropertiesTests(Player player)
        {
            Debug.Log($"test {player.GetDebugLabel()}");

            const string intPropName = "INT";
            const string stringPropName = "STR";
            const int intValue = 123;
            const int newIntValue = 321;
            const string stringValue = "123";
            const string newStringValue = "321";

            yield return null;
            var hasProp = player.HasCustomProperty(intPropName);
            Assert.IsFalse(hasProp);

            player.SetCustomProperty(intPropName, intValue);
            yield return null;

            hasProp = player.HasCustomProperty(intPropName);
            Assert.IsTrue(hasProp);
            yield return null;

            player.SetCustomProperty(stringPropName, stringValue);
            yield return null;

            hasProp = player.HasCustomProperty(stringPropName);
            Assert.IsTrue(hasProp);
            yield return null;
        }

        private static bool TimedWait(Func<bool> action, float timeoutTime)
        {
            return Time.time > timeoutTime || action();
        }
    }
}