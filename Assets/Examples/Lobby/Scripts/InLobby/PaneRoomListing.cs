﻿using Photon.Pun;
using Photon.Realtime;
using Prg.Scripts.Common.Photon;
using System;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace Examples.Lobby.Scripts.InLobby
{
    /// <summary>
    /// Shows list of (open/closed) rooms and buttons for creating a new room or joining existing one.
    /// </summary>
    public class PaneRoomListing : MonoBehaviour
    {
        [SerializeField] private Text title;
        [SerializeField] private Button templateButton;
        [SerializeField] private Transform buttonParent;

        private PhotonRoomList photonRoomList;

        private void Start()
        {
            title.text = $"Welcome to {Application.productName}";
            templateButton.onClick.AddListener(createRoomForMe);
        }

        private void OnEnable()
        {
            photonRoomList = FindObjectOfType<PhotonRoomList>();
            if (photonRoomList != null)
            {
                if (PhotonNetwork.InLobby)
                {
                    updateStatus();
                }
                photonRoomList.roomsUpdated += updateStatus;
            }
        }

        private void OnDisable()
        {
            if (photonRoomList != null)
            {
                photonRoomList.roomsUpdated -= updateStatus;
                photonRoomList = null;
            }
            deleteExtraButtons(buttonParent);
        }

        private void updateStatus()
        {
            if (!PhotonNetwork.InLobby)
            {
                deleteExtraButtons(buttonParent);
                return;
            }
            var rooms = photonRoomList.currentRooms.ToList();
            rooms.Sort((a, b) => string.Compare(a.Name, b.Name, StringComparison.Ordinal));
            Debug.Log($"updateStatus enter {PhotonNetwork.NetworkClientState} buttons: {buttonParent.childCount} rooms: {rooms.Count}");

            // Synchronize button count with room count.
            while (buttonParent.childCount < rooms.Count)
            {
                addButton(buttonParent, templateButton);
            }
            // Update button captions
            for (var i = 0; i < rooms.Count; ++i)
            {
                var room = rooms[i];
                var buttonObject = buttonParent.GetChild(i).gameObject;
                if (!buttonObject.activeSelf)
                {
                    buttonObject.SetActive(true);
                }
                var button = buttonObject.GetComponent<Button>();
                update(button, room);
            }
            if (buttonParent.childCount > rooms.Count)
            {
                for (var i = rooms.Count; i < buttonParent.childCount; ++i)
                {
                    var buttonObject = buttonParent.GetChild(i).gameObject;
                    if (buttonObject.activeSelf)
                    {
                        buttonObject.SetActive(false);
                    }
                }
            }
            Debug.Log($"updateStatus exit {PhotonNetwork.NetworkClientState} buttons: {buttonParent.childCount} rooms: {rooms.Count}");
        }

        private static void createRoomForMe()
        {
            Debug.Log("createRoomForMe");
            PhotonLobby.createRoom($"Room{DateTime.Now.Second:00}");
        }

        private void joinRoom(string roomName)
        {
            Debug.Log($"joinRoom '{roomName}'");
            var rooms = photonRoomList.currentRooms.ToList();
            foreach (var roomInfo in rooms)
            {
                if (roomInfo.Name == roomName && !roomInfo.RemovedFromList && roomInfo.IsOpen)
                {
                    PhotonLobby.joinRoom(roomInfo);
                }
            }
        }

        private static void addButton(Transform parent, Button template)
        {
            var templateParent = template.gameObject;
            var instance = Instantiate(templateParent, parent);
            Debug.Log($"duplicate {instance.name}");
        }

        private void update(Button button, RoomInfo room)
        {
            var text = button.GetComponentInChildren<Text>();
            var roomText = $"{room.Name}";
            if (roomText.Length > 21)
            {
                roomText = roomText.Substring(0, 20) + "…";
            }
            if (room.IsOpen)
            {
                roomText += $" ({room.PlayerCount})";
                roomText = $"<color=blue>{roomText}</color>";
            }
            else
            {
                roomText += " (closed)";
                roomText = $"<color=brown>{roomText}</color>";
            }
            Debug.Log($"update '{text.text}' -> '{roomText}'");
            text.text = roomText;
            button.onClick.RemoveAllListeners();
            if (room.IsOpen)
            {
                button.onClick.AddListener(() => joinRoom(room.Name));
            }
        }

        private static void deleteExtraButtons(Transform parent)
        {
            var childCount = parent.childCount;
            for (var i = childCount - 1; i >= 0; --i)
            {
                var child = parent.GetChild(i).gameObject;
                Debug.Log($"Destroy {child.name}");
                Destroy(child);
            }
        }
    }
}