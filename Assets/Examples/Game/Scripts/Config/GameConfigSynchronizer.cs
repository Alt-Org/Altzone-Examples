using Photon.Pun;
using Prg.Scripts.Common.Photon;
using System;
using System.IO;
using System.Reflection;
using UnityEngine;

namespace Examples.Game.Scripts.Config
{
    [Flags] public enum What
    {
        None = 0,
        All = 1,
        Features = 2,
        Variables = 4,
    }

    /// <summary>
    /// Synchronize runtime game config over network.
    /// </summary>
    /// <remarks>
    /// Only Master Client can do this while in a room.
    /// </remarks>
    public class GameConfigSynchronizer : MonoBehaviour
    {
        private const int photonEventCode = PhotonEventDispatcher.eventCodeBase + 4; // synchronize game config
        private const byte endByte = 0xFE;

        public static void synchronize(What what)
        {
            if (!PhotonNetwork.InRoom || !PhotonNetwork.IsMasterClient)
            {
                throw new UnityException("only master client can synchronize in a room");
            }
            if (what.HasFlag(What.All) || what.HasFlag(What.Features))
            {
                Get().synchronizeFeatures();
            }
            if (what.HasFlag(What.All) || what.HasFlag(What.Variables))
            {
                Get().synchronizeVariables();
            }
        }

        private static GameConfigSynchronizer Get()
        {
            if (_Instance == null)
            {
                _Instance = FindObjectOfType<GameConfigSynchronizer>();
                if (_Instance == null)
                {
                    _Instance = UnityExtensions.CreateGameObjectAndComponent<GameConfigSynchronizer>(nameof(GameConfigSynchronizer),
                        isDontDestroyOnLoad: true);
                }
            }
            return _Instance;
        }

        private static GameConfigSynchronizer _Instance;

        private PhotonEventDispatcher photonEventDispatcher;

        private void Awake()
        {
            photonEventDispatcher = PhotonEventDispatcher.Get();
            photonEventDispatcher.registerEventListener(photonEventCode, data =>
            {
                if (data.CustomData is byte[] bytes)
                {
                    if (bytes.Length < 3)
                    {
                        throw new UnityException("invalid synchronization message length: " + bytes.Length);
                    }
                    var lastByte = bytes[bytes.Length - 1];
                    if (lastByte != endByte)
                    {
                        throw new UnityException("invalid synchronization message end: " + lastByte);
                    }
                    var firstByte = bytes[0];
                    if (firstByte == (byte) What.Features)
                    {
                        readFeatures(bytes);
                    }
                    else if (firstByte == (byte) What.Variables)
                    {
                        readVariables(bytes);
                    }
                    else
                    {
                        throw new UnityException("invalid synchronization message start: " + firstByte);
                    }
                }
            });
        }

        private void synchronizeFeatures()
        {
            var features = RuntimeGameConfig.Get().features;
            var countFieldsToWrite = 0;
            using (var stream = new MemoryStream())
            {
                using (var writer = new BinaryWriter(stream))
                {
                    writer.Write((byte) What.Features);
                    countFieldsToWrite += 1;
                    writer.Write(features.isRotateGameCamera);
                    countFieldsToWrite += 1;
                    writer.Write(features.isSPawnMiniBall);
                    countFieldsToWrite += 1;
                    writer.Write(features.isActivateTeamWithBall);
                    writer.Write(endByte);
                }
                var bytes = stream.ToArray();
                Debug.Log($"synchronizeFeatures data length {bytes.Length} fields {countFieldsToWrite}");
                Debug.Log($"data> {string.Join(", ", bytes)}");
                var type = features.GetType();
                var countFields = type.GetFields(BindingFlags.Public | BindingFlags.Instance).Length;
                if (countFieldsToWrite != countFields)
                {
                    throw new UnityException($"mismatch in type {type} fields {countFields} and written fields {countFieldsToWrite}");
                }
                photonEventDispatcher.RaiseEvent(photonEventCode, bytes);
            }
        }

        private static void readFeatures(byte[] bytes)
        {
            Debug.Log($"readFeatures data length {bytes.Length}");
            Debug.Log($"data< {string.Join(", ", bytes)}");
            var features = new GameFeatures();
            using (var stream = new MemoryStream(bytes))
            {
                using (var reader = new BinaryReader(stream))
                {
                    reader.ReadByte(); // skip first
                    features.isRotateGameCamera = reader.ReadBoolean();
                    features.isRotateGameCamera = reader.ReadBoolean();
                    features.isRotateGameCamera = reader.ReadBoolean();
                    reader.ReadByte(); // skip last
                }
            }
            RuntimeGameConfig.Get().features = features;
        }

        private void synchronizeVariables()
        {
            var variables = RuntimeGameConfig.Get().variables;
            var countFieldsToWrite = 0;
            using (var stream = new MemoryStream())
            {
                using (var writer = new BinaryWriter(stream))
                {
                    writer.Write((byte) What.Variables);
                    countFieldsToWrite += 1;
                    writer.Write(variables.ballMoveSpeed);
                    countFieldsToWrite += 1;
                    writer.Write(variables.ballLerpSmoothingFactor);
                    countFieldsToWrite += 1;
                    writer.Write(variables.ballTeleportDistance);
                    countFieldsToWrite += 1;
                    writer.Write(variables.playerMoveSpeed);
                    countFieldsToWrite += 1;
                    writer.Write(variables.playerSqrMinRotationDistance);
                    countFieldsToWrite += 1;
                    writer.Write(variables.playerSqrMaxRotationDistance);
                    writer.Write(endByte);
                }
                var bytes = stream.ToArray();
                Debug.Log($"synchronizeVariables data length {bytes.Length} fields {countFieldsToWrite}");
                Debug.Log($"data> {string.Join(", ", bytes)}");
                var type = variables.GetType();
                var countFields = type.GetFields(BindingFlags.Public | BindingFlags.Instance).Length;
                if (countFieldsToWrite != countFields)
                {
                    throw new UnityException($"mismatch in type {type} fields {countFields} and written fields {countFieldsToWrite}");
                }
                photonEventDispatcher.RaiseEvent(photonEventCode, bytes);
            }
        }

        private static void readVariables(byte[] bytes)
        {
            Debug.Log($"readVariables data length {bytes.Length}");
            Debug.Log($"data< {string.Join(", ", bytes)}");
            var variables = new GameVariables();
            using (var stream = new MemoryStream(bytes))
            {
                using (var reader = new BinaryReader(stream))
                {
                    reader.ReadByte(); // skip first
                    variables.ballMoveSpeed = reader.ReadSingle();
                    variables.ballLerpSmoothingFactor = reader.ReadSingle();
                    variables.ballTeleportDistance = reader.ReadSingle();
                    variables.playerMoveSpeed = reader.ReadSingle();
                    variables.playerSqrMinRotationDistance = reader.ReadSingle();
                    variables.playerSqrMaxRotationDistance = reader.ReadSingle();
                    reader.ReadByte(); // skip last
                }
            }
            RuntimeGameConfig.Get().variables = variables;
        }
    }
}