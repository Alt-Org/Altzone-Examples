using UnityEngine;

namespace Altzone.Scripts.Config
{
    /// <summary>
    /// Editable persistent settings for the game.
    /// </summary>
    /// <remarks>
    /// Create these in <c>Resources</c> folder with name "PersistentGameSettings" so they can be loaded when needed first time.
    /// </remarks>
    [CreateAssetMenu(menuName = "ALT-Zone/PersistentGameSettings")]
    public class PersistentGameSettings : ScriptableObject
    {
        [Header("Game Features")] public GameFeatures features;
        [Header("Game Variables")] public GameVariables variables;
        [Header("Game Prefabs")] public GamePrefabs prefabs;
    }
}