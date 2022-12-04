using System;
using UnityEngine;
using UnityEngine.Assertions;

namespace Altzone.Scripts.Config.ScriptableObjects
{
    /// <summary>
    /// Editable persistent settings for the game.
    /// </summary>
    /// <remarks>
    /// Create these in <c>Resources</c> folder with name "GameSettings" so they can be loaded when needed first time.
    /// </remarks>
    // [CreateAssetMenu(menuName = "ALT-Zone/GameSettings", fileName = "GameSettings")]
    internal class GameSettings : ScriptableObject
    {
        private const string GameSettingsName = "GameSettings";

        [Header("Game Features")] public GameFeatures _features;

        [Header("Game Constraints")] public GameConstraints _constraints;

        [Header("Game Variables")] public GameVariables _variables;

        [Header("Characters")] public Characters _characters;

        internal static GameSettings Load()
        {
            var gameSettings = Resources.Load<GameSettings>(GameSettingsName);
            Assert.IsNotNull(gameSettings, $"ASSET '{GameSettingsName}' NOT FOUND");
            return gameSettings;
        }
    }

    #region GameSettings "Parts"

    /// <summary>
    /// Game features that can be toggled on and off.
    /// </summary>
    /// <remarks>
    /// Note that these member variables can be serialized over network and thus must be internally serializable.
    /// </remarks>
    [Serializable]
    public class GameFeatures
    {
        /// <summary>
        /// Rotate game camera for upper team so they see their own game area in lower part of the screen.
        /// </summary>
        public bool _isRotateGameCamera;

        /// <summary>
        /// Is local player team color always "blue" team side.
        /// </summary>
        public bool _isRotateGamePlayArea;

        /// <summary>
        /// Spawn mini ball aka diamonds.
        /// </summary>
        public bool _isSPawnMiniBall;

        /// <summary>
        /// Is shield always on when team has only one player (for testing).
        /// </summary>
        public bool _isSinglePlayerShieldOn;

        /// <summary>
        /// Is bricks visible.
        /// </summary>
        public bool _isBricksVisible;
    }

    /// <summary>
    /// Game constraints that that control the workings of the game.
    /// </summary>
    [Serializable]
    public class GameConstraints
    {
        [Header("Player Name"), Min(3)] public int _minPlayerNameLength = 3;
        [Min(16)] public int _maxPlayerNameLength = 16;
    }

    /// <summary>
    /// Game variables that control game play somehow.
    /// </summary>
    /// <remarks>
    /// Note that these member variables can be serialized over network and thus must be internally serializable.
    /// </remarks>
    [Serializable]
    public class GameVariables
    {
        [Header("Battle"), Min(1)] public int _roomStartDelay;
        public int _headScoreToWin;
        public int _wallScoreToWin;

        [Header("Ball")] public float _ballMoveSpeedMultiplier;
        public float _ballLerpSmoothingFactor;
        public float _ballTeleportDistance;
        public float _minSlingShotDistance;
        public float _maxSlingShotDistance;
        [Min(1)] public int _ballRestartDelay;

        [Header("Player")] public float _playerMoveSpeedMultiplier;
        public float _playerSqrMinRotationDistance;
        public float _playerSqrMaxRotationDistance;

        [Header("Shield")] public float _shieldDistanceMultiplier;
    }

    ///<summary>
    /// Character model attribute editing for Unity Editor
    /// </summary>  
    [Serializable]
    public class Characters
    {
    }

    #endregion
}