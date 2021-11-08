using System;
using System.Collections;
using System.Threading;
using NUnit.Framework;
using Photon.Pun;
using Prg.Scripts.Common.Photon;
using UnityEngine;
using UnityEngine.TestTools;
using Assert = UnityEngine.Assertions.Assert;

//using Hashtable = ExitGames.Client.Photon.Hashtable;

namespace Tests.PlayMode
{
    public class PhotonTestScript
    {
        private const string PlayerName = "Teppo";
        private const string RoomName = "Test1";
        private const float Timeout = 1.0f;

        [OneTimeSetUp]
        public void OneTimeSetUp()
        {
            Debug.Log($"OneTimeSetUp start {PhotonWrapper.NetworkClientState}");
            PhotonLobby.connect(PlayerName);
            Debug.Log($"OneTimeSetUp exit {PhotonWrapper.NetworkClientState}");
        }

        [OneTimeTearDown]
        public void OneTimeTearDown()
        {
            Debug.Log($"OneTimeTearDown start {PhotonWrapper.NetworkClientState}");
            PhotonLobby.disconnect();
            Debug.Log($"OneTimeTearDown exit {PhotonWrapper.NetworkClientState}");
        }

        // A UnityTest behaves like a coroutine in Play Mode. In Edit Mode you can use
        // `yield return null;` to skip a frame.
        [UnityTest]
        public IEnumerator ConnectToPhoton()
        {
            Debug.Log($"test start {PhotonWrapper.NetworkClientState}");

            var timeoutTime0 = Time.time + Timeout;
            yield return new WaitUntil(() => TimedWait(() => PhotonWrapper.CanJoinLobby, timeoutTime0));
            Assert.IsTrue(PhotonWrapper.CanJoinLobby, "Can not join lobby");

            PhotonLobby.joinLobby();

            var timeoutTime1 = Time.time + Timeout;
            yield return new WaitUntil(() => TimedWait(() => PhotonWrapper.InLobby, timeoutTime1));
            Assert.IsTrue(PhotonNetwork.InLobby, "Not in lobby");

            //Hashtable customRoomProperties = null;
            //string[] lobbyPropertyNames = null;
            //PhotonLobby.joinOrCreateRoom(roomName, customRoomProperties, lobbyPropertyNames);

            var timeoutTime2 = Time.time + Timeout;
            yield return new WaitUntil(() => TimedWait(() => PhotonWrapper.InRoom, timeoutTime2));
            Assert.IsTrue(PhotonNetwork.InRoom, "Not in room");

            Debug.Log($"test end {PhotonWrapper.NetworkClientState}");
        }

        private static bool TimedWait(Func<bool> action, float timeoutTime)
        {
            return Time.time > timeoutTime || action();
        }
    }
}