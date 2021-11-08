using System;
using System.Collections;
using NUnit.Framework;
using Photon.Pun;
using Prg.Scripts.Common.Photon;
using UnityEngine;
using UnityEngine.TestTools;

namespace Tests.PlayMode
{
    public class PhotonConnectionTest
    {
        private const string PlayerName = "Teppo";
        private const string RoomName = "Test1";
        private const float Timeout = 2.0f;

        [UnityTest]
        public IEnumerator ConnectToPhoton()
        {
            Debug.Log($"test start {PhotonWrapper.NetworkClientState}");

            PhotonLobby.connect(PlayerName);
            yield return null;

            var canJoinLobbyTimeout = Time.time + Timeout;
            yield return new WaitUntil(() => TimedWait(() => PhotonWrapper.CanJoinLobby, canJoinLobbyTimeout));
            Assert.That(PhotonWrapper.CanJoinLobby, Is.True, "Can not join lobby");

            PhotonLobby.joinLobby();
            yield return null;

            var inLobbyTimeout = Time.time + Timeout;
            yield return new WaitUntil(() => TimedWait(() => PhotonWrapper.InLobby, inLobbyTimeout));
            Assert.That(PhotonNetwork.InLobby, Is.True, "Not in lobby");

            PhotonLobby.joinOrCreateRoom(RoomName, null, null);

            var inRoomTimeout = Time.time + Timeout;
            yield return new WaitUntil(() => TimedWait(() => PhotonWrapper.InRoom, inRoomTimeout));
            Assert.That(PhotonNetwork.InRoom, Is.True, "Not in room");

            PhotonLobby.disconnect();
            yield return null;

            var isPhotonReadyTimeout = Time.time + Timeout;
            yield return new WaitUntil(() => TimedWait(() => PhotonWrapper.IsPhotonReady, isPhotonReadyTimeout));
            Assert.That(PhotonNetwork.IsConnected, Is.False, "Is connected");

            Debug.Log($"test end {PhotonWrapper.NetworkClientState}");
        }

        private static bool TimedWait(Func<bool> action, float timeoutTime)
        {
            return Time.time > timeoutTime || action();
        }
    }
}