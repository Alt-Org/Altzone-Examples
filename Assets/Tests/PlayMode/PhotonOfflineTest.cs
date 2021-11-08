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
        public IEnumerator CustomPropertiesTests()
        {
            Debug.Log($"test start {PhotonWrapper.NetworkClientState}");

            var inRoomTimeout = Time.time + Timeout;
            yield return new WaitUntil(() => TimedWait(() => PhotonWrapper.InRoom, inRoomTimeout));
            Assert.That(PhotonNetwork.InRoom, Is.True, "Not in room");

            yield return RoomCustomPropertiesTests(PhotonNetwork.CurrentRoom);
            yield return null;
            yield return PlayerCustomPropertiesTests(PhotonNetwork.LocalPlayer);

            Debug.Log($"test end {PhotonWrapper.NetworkClientState}");
        }

        private static IEnumerator RoomCustomPropertiesTests(Room room)
        {
            Debug.Log($"test start {room.GetDebugLabel()}");

            const string intPropName = "INT";
            const string stringPropName = "STR";
            const int intValue = 123;
            const int newIntValue = 321;
            const string stringValue = "123";
            const string newStringValue = "321";

            yield return null;
            var hasProp = room.HasCustomProperty(intPropName);
            Assert.That(hasProp, Is.False);

            // int tests - except SafeSetCustomProperty failure

            room.SetCustomProperty(intPropName, intValue);
            yield return null;

            hasProp = room.HasCustomProperty(intPropName);
            Assert.That(hasProp, Is.True);
            yield return null;

            object propValue = room.GetCustomProperty(intPropName, -1);
            Assert.That(propValue, Is.EqualTo(intValue));
            yield return null;

            room.SafeSetCustomProperty(intPropName, newIntValue, intValue);
            yield return null;

            propValue = room.GetCustomProperty(intPropName, -1);
            Assert.That(propValue, Is.EqualTo(newIntValue));
            yield return null;

            room.RemoveCustomProperty(intPropName);
            yield return null;

            hasProp = room.HasCustomProperty(intPropName);
            Assert.That(hasProp, Is.False);
            yield return null;

            // string tests - except SafeSetCustomProperty failure

            room.SetCustomProperty(stringPropName, stringValue);
            yield return null;

            hasProp = room.HasCustomProperty(stringPropName);
            Assert.That(hasProp, Is.True);
            yield return null;

            propValue = room.GetCustomProperty(stringPropName, string.Empty);
            Assert.That(propValue, Is.EqualTo(stringValue));
            yield return null;

            room.SafeSetCustomProperty(stringPropName, newStringValue, stringValue);
            yield return null;

            propValue = room.GetCustomProperty(stringPropName, string.Empty);
            Assert.That(propValue, Is.EqualTo(newStringValue));
            yield return null;

            room.RemoveCustomProperty(stringPropName);
            yield return null;

            hasProp = room.HasCustomProperty(stringPropName);
            Assert.That(hasProp, Is.False);

            Debug.Log($"test end {room.GetDebugLabel()}");
        }

        private static IEnumerator PlayerCustomPropertiesTests(Player player)
        {
            Debug.Log($"test start {player.GetDebugLabel()}");

            const string intPropName = "INT";
            const string stringPropName = "STR";
            const int intValue = 123;
            const int newIntValue = 321;
            const string stringValue = "123";
            const string newStringValue = "321";

            yield return null;
            var hasProp = player.HasCustomProperty(intPropName);
            Assert.That(hasProp, Is.False);

            // int tests - except SafeSetCustomProperty failure

            player.SetCustomProperty(intPropName, intValue);
            yield return null;

            hasProp = player.HasCustomProperty(intPropName);
            Assert.That(hasProp, Is.True);
            yield return null;

            object propValue = player.GetCustomProperty(intPropName, -1);
            Assert.That(propValue, Is.EqualTo(intValue));
            yield return null;

            player.SafeSetCustomProperty(intPropName, newIntValue, intValue);
            yield return null;

            propValue = player.GetCustomProperty(intPropName, -1);
            Assert.That(propValue, Is.EqualTo(newIntValue));
            yield return null;

            player.RemoveCustomProperty(intPropName);
            yield return null;

            hasProp = player.HasCustomProperty(intPropName);
            Assert.That(hasProp, Is.False);
            yield return null;

            // string tests - except SafeSetCustomProperty failure

            player.SetCustomProperty(stringPropName, stringValue);
            yield return null;

            hasProp = player.HasCustomProperty(stringPropName);
            Assert.That(hasProp, Is.True);
            yield return null;

            propValue = player.GetCustomProperty(stringPropName, string.Empty);
            Assert.That(propValue, Is.EqualTo(stringValue));
            yield return null;

            player.SafeSetCustomProperty(stringPropName, newStringValue, stringValue);
            yield return null;

            propValue = player.GetCustomProperty(stringPropName, string.Empty);
            Assert.That(propValue, Is.EqualTo(newStringValue));
            yield return null;

            player.RemoveCustomProperty(stringPropName);
            yield return null;

            hasProp = player.HasCustomProperty(stringPropName);
            Assert.That(hasProp, Is.False);

            Debug.Log($"test end {player.GetDebugLabel()}");
        }

        private static bool TimedWait(Func<bool> action, float timeoutTime)
        {
            return Time.time > timeoutTime || action();
        }
    }
}