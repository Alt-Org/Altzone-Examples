using Examples2.Scripts.Battle.Ball;
using Examples2.Scripts.Battle.interfaces;
using UnityEngine;

namespace Examples2.Scripts.Battle.Factory
{
    /// <summary>
    /// Helper class to find all actors in the game in order to hide dependencies to actual <c>MonoBehaviour</c> implementations.
    /// </summary>
    /// <remarks>
    /// This class is very sensitive to <c>script execution order</c>!
    /// </remarks>
    internal static class Context
    {
        internal static IBall getBall => Object.FindObjectOfType<BallActor>();
    }
}