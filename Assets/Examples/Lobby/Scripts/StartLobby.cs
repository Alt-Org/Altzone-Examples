﻿using Prg.Scripts.Common.Photon;
using System;
using UnityEngine;

namespace Examples.Lobby.Scripts
{
    /// <summary>
    /// Helper class to enter Photon lobby.
    /// </summary>
    public class StartLobby : MonoBehaviour
    {
        [SerializeField] private GameObject inLobby;
        [SerializeField] private GameObject inRoom;

        private void Start()
        {
            inLobby.SetActive(false);
            inRoom.SetActive(false);
        }

        private void Update()
        {
            if (PhotonWrapper.InLobby)
            {
                inLobby.SetActive(true);
                enabled = false;
                return;
            }
            if (PhotonWrapper.InRoom)
            {
                PhotonLobby.leaveRoom();
                return;
            }
            if (PhotonWrapper.CanJoinLobby)
            {
                PhotonLobby.joinLobby();
                return;
            }
            if (PhotonWrapper.CanConnect)
            {
                var randomPlayer = $"Player{DateTime.Now.Second:00}";
                PhotonLobby.connect(randomPlayer);
            }
        }
    }
}
