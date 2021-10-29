using Prg.Scripts.Common.Photon;
using System;
using System.IO;
using System.Reflection;
using UnityEngine;

namespace Examples.Config.Scripts
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
        private const int msgSynchronize = PhotonEventDispatcher.eventCodeBase + 0;
        private const byte endByte = 0xFE;
        private const int overheadBytes = 2;

        public static void listen()
        {
            Get(); // Instantiate our private instance for listening synchronize events
        }

        public static void synchronize(What what)
        {
            if (!PhotonWrapper.InRoom || !PhotonWrapper.IsMasterClient)
            {
                throw new UnityException("only master client can synchronize in a room");
            }
            if (what.HasFlag(What.All) || what.HasFlag(What.Features))
            {
                Get().sendSynchronizeFeatures((byte)What.Features, endByte);
            }
            if (what.HasFlag(What.All) || what.HasFlag(What.Variables))
            {
                Get().sendSynchronizeVariables((byte)What.Variables, endByte);
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
            photonEventDispatcher.registerEventListener(msgSynchronize, data => onSynchronize(data.CustomData));
        }

        private void OnDestroy()
        {
            _Instance = null;
        }

        private static void onSynchronize(object data)
        {
            if (data is byte[] bytes)
            {
                if (bytes.Length < 3)
                {
                    throw new UnityException($"invalid synchronization message length: {bytes.Length}");
                }
                var lastByte = bytes[bytes.Length - 1];
                if (lastByte != endByte)
                {
                    throw new UnityException($"invalid synchronization message end: {lastByte}");
                }
                var firstByte = bytes[0];
                if (firstByte == (byte)What.Features)
                {
                    readFeatures(bytes);
                }
                else if (firstByte == (byte)What.Variables)
                {
                    readVariables(bytes);
                }
                else
                {
                    throw new UnityException($"invalid synchronization message start: {firstByte}");
                }
            }
        }

        private void sendSynchronizeFeatures(byte first, byte last)
        {
            var features = RuntimeGameConfig.Get().features;
            using (var stream = new MemoryStream())
            {
                using (var writer = new BinaryWriter(stream))
                {
                    writer.Write(first);
                    writer.Write(features.isRotateGameCamera);
                    writer.Write(features.isLocalPLayerOnTeamBlue);
                    writer.Write(features.isSPawnMiniBall);
                    writer.Write(features.isSinglePlayerShieldOn);
                    writer.Write(last);
                }
                var bytes = stream.ToArray();
                var type = features.GetType();
                var fieldsLength = countFieldsByteSize(type, out var fieldCount);
                //--Debug.Log($"send data> {bytes.Length}({fieldCount}): {string.Join(", ", bytes)}");
                if (bytes.Length != fieldsLength)
                {
                    throw new UnityException($"mismatch in type {type} fields size {fieldsLength} and written fields size {bytes.Length}");
                }
                photonEventDispatcher.RaiseEvent(msgSynchronize, bytes);
            }
        }

        private static void readFeatures(byte[] bytes)
        {
            //--Debug.Log($"recv data< {bytes.Length}: {string.Join(", ", bytes)}");
            var features = new GameFeatures();
            using (var stream = new MemoryStream(bytes))
            {
                using (var reader = new BinaryReader(stream))
                {
                    reader.ReadByte(); // skip first
                    features.isRotateGameCamera = reader.ReadBoolean();
                    features.isLocalPLayerOnTeamBlue = reader.ReadBoolean();
                    features.isSPawnMiniBall = reader.ReadBoolean();
                    features.isSinglePlayerShieldOn = reader.ReadBoolean();
                    reader.ReadByte(); // skip last
                }
            }
            RuntimeGameConfig.Get().features = features;
        }

        private void sendSynchronizeVariables(byte first, byte last)
        {
            var variables = RuntimeGameConfig.Get().variables;
            using (var stream = new MemoryStream())
            {
                using (var writer = new BinaryWriter(stream))
                {
                    writer.Write(first);
                    writer.Write(variables.roomStartDelay);
                    writer.Write(variables.ballMoveSpeedMultiplier);
                    writer.Write(variables.ballLerpSmoothingFactor);
                    writer.Write(variables.ballTeleportDistance);
                    writer.Write(variables.minSlingShotDistance);
                    writer.Write(variables.maxSlingShotDistance);
                    writer.Write(variables.ballRestartDelay);
                    writer.Write(variables.playerMoveSpeedMultiplier);
                    writer.Write(variables.playerSqrMinRotationDistance);
                    writer.Write(variables.playerSqrMaxRotationDistance);
                    writer.Write(variables.shieldDistanceMultiplier);
                    writer.Write(last);
                }
                var bytes = stream.ToArray();
                var type = variables.GetType();
                var fieldsLength = countFieldsByteSize(type, out var fieldCount);
                //--Debug.Log($"send data> {bytes.Length}({fieldCount}): {string.Join(", ", bytes)}");
                if (bytes.Length != fieldsLength)
                {
                    throw new UnityException($"mismatch in type {type} fields size {fieldsLength} and written fields size {bytes.Length}");
                }
                photonEventDispatcher.RaiseEvent(msgSynchronize, bytes);
            }
        }

        private static void readVariables(byte[] bytes)
        {
            //--Debug.Log($"recv data< {bytes.Length}: {string.Join(", ", bytes)}");
            var variables = new GameVariables();
            using (var stream = new MemoryStream(bytes))
            {
                using (var reader = new BinaryReader(stream))
                {
                    reader.ReadByte(); // skip first
                    variables.roomStartDelay = reader.ReadInt32();
                    variables.ballMoveSpeedMultiplier = reader.ReadSingle();
                    variables.ballLerpSmoothingFactor = reader.ReadSingle();
                    variables.ballTeleportDistance = reader.ReadSingle();
                    variables.minSlingShotDistance = reader.ReadSingle();
                    variables.maxSlingShotDistance = reader.ReadSingle();
                    variables.ballRestartDelay = reader.ReadInt32();
                    variables.playerMoveSpeedMultiplier = reader.ReadSingle();
                    variables.playerSqrMinRotationDistance = reader.ReadSingle();
                    variables.playerSqrMaxRotationDistance = reader.ReadSingle();
                    variables.shieldDistanceMultiplier = reader.ReadSingle();
                    reader.ReadByte(); // skip last
                }
            }
            RuntimeGameConfig.Get().variables = variables;
        }

        private static int countFieldsByteSize(Type type, out int fieldCount)
        {
            var fields = type.GetFields(BindingFlags.Public | BindingFlags.Instance);
            var countBytes = overheadBytes; // stream start and end bytes
            foreach (var fieldInfo in fields)
            {
                var fieldTypeName = fieldInfo.FieldType.Name;
                switch (fieldTypeName)
                {
                    case "Boolean":
                        countBytes += 1;
                        break;
                    case "Int32":
                        countBytes += 4;
                        break;
                    case "Single":
                        countBytes += 4;
                        break;
                    default:
                        throw new UnityException($"unknown field type: {fieldTypeName}");
                }
            }
            fieldCount = fields.Length;
            return countBytes;
        }
    }
}