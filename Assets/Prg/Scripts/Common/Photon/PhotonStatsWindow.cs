using Photon.Pun;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Prg.Scripts.Common.Photon
{
    /// <summary>
    /// Helper OnGUI window to show some Photon related info as "overlay" window.
    /// </summary>
    public class PhotonStatsWindow : MonoBehaviour
    {
        public bool _visible;
        public Key _controlKey = Key.F2;

#if DEVELOPMENT_BUILD || UNITY_EDITOR
        private int _windowId;
        private Rect _windowRect;
        private string _windowTitle;
        private bool _hasStyles;
        private GUIStyle _guiButtonStyle;
        private GUIStyle _guiLabelStyle;

        private void OnEnable()
        {
            _windowId = (int)DateTime.Now.Ticks;
            _windowRect = new Rect(0, 0, Screen.width, Screen.height);
            _windowTitle = $"({_controlKey}) Photon";
        }

        private void Update()
        {
            if (Keyboard.current[_controlKey].isPressed)
            {
                ToggleWindowState();
            }
        }

        private void ToggleWindowState()
        {
            _visible = !_visible;
        }

        private void OnGUI()
        {
            if (!_visible)
            {
                return;
            }
            if (!_hasStyles)
            {
                _hasStyles = true;
                _guiButtonStyle = new GUIStyle(GUI.skin.button) { fontSize = 20 };
                _guiLabelStyle = new GUIStyle(GUI.skin.label) { fontSize = 24 };
            }
            _windowRect = GUILayout.Window(_windowId, _windowRect, DebugWindow, _windowTitle);
        }

        private void DebugWindow(int windowId)
        {
            string label;
            var inRoom = PhotonNetwork.InRoom;
            if (inRoom)
            {
                var room = PhotonNetwork.CurrentRoom;
                label = $"{PhotonNetwork.LocalPlayer.NickName} | {room.Name}" +
                        $"{(room.IsVisible ? string.Empty : ",hidden")}" +
                        $"{(room.IsOpen ? string.Empty : ",closed")} " +
                        $"{(room.PlayerCount == 1 ? "1 player" : $"{room.PlayerCount} players")}" +
                        $"{(room.MaxPlayers == 0 ? string.Empty : $" (max {room.MaxPlayers})")}";
            }
            else if (PhotonNetwork.InLobby)
            {
                label = $"Lobby: rooms {PhotonNetwork.CountOfRooms}, players {PhotonNetwork.CountOfPlayers}";
            }
            else
            {
                label = $"Photon: {PhotonNetwork.NetworkClientState}";
            }
            if (GUILayout.Button(label, _guiButtonStyle))
            {
                ToggleWindowState();
            }
            if (inRoom)
            {
                label = "Props:";
                var room = PhotonNetwork.CurrentRoom;
                var props = room.CustomProperties;
                var keys = props.Keys.ToList();
                keys.Sort((a, b) => string.Compare(a.ToString(), b.ToString(), StringComparison.Ordinal));
                foreach (var key in keys)
                {
                    if ("curScn".Equals(key))
                    {
                        // Skip current scene name.
                        continue;
                    }
                    var propValue = props[key];
                    label += $"\r\n{key}={propValue} [{ShortTypeName(propValue.GetType())}]";
                }
                label += "\r\nPlayers:";
                foreach (var player in room.GetPlayersByActorNumber())
                {
                    var text = player.GetDebugLabel(verbose: false);
                    label += $"\r\n{text}";
                    props = player.CustomProperties;
                    if (props.Count > 0)
                    {
                        keys = props.Keys.ToList();
                        keys.Sort((a, b) => string.Compare(a.ToString(), b.ToString(), StringComparison.Ordinal));
                        foreach (var key in keys)
                        {
                            var propValue = props[key];
                            label += $"\r\n{key}={propValue} [{ShortTypeName(propValue.GetType())}]";
                        }
                    }
                }
            }
            label += $"\r\nPhoton v='{PhotonLobby.GameVersion}'";
            label += $"\r\nSend rate={PhotonNetwork.SendRate} ser rate={PhotonNetwork.SerializationRate}";
            GUILayout.Label(label, _guiLabelStyle);
        }

        private static readonly Dictionary<Type, string> TypeMap = new Dictionary<Type, string>()
        {
            // https://docs.microsoft.com/en-us/dotnet/csharp/language-reference/builtin-types/built-in-types
            { typeof(bool), "bool" },
            { typeof(byte), "byte" },
            { typeof(sbyte), "sbyte" },
            { typeof(char), "char" },
            { typeof(decimal), "decimal" },
            { typeof(double), "double" },
            { typeof(float), "float" },
            { typeof(int), "int" },
            { typeof(uint), "uint" },
            { typeof(long), "long" },
            { typeof(ulong), "ulong" },
            { typeof(short), "short" },
            { typeof(ushort), "ushort" },
            { typeof(string), "str" },
        };

        private static string ShortTypeName(Type type)
        {
            return TypeMap.TryGetValue(type, out var name) ? name : type.Name;
        }
#endif
    }
}