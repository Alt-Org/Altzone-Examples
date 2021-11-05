using Examples2.Scripts.Battle.Ball;
using Examples2.Scripts.Battle.interfaces;
using Examples2.Scripts.Battle.PlayerConnect;
using Examples2.Scripts.Battle.Room;
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
        internal static IBall GetBall => Object.FindObjectOfType<BallActor>();

        internal static IBrickManager GetBrickManager => Object.FindObjectOfType<BrickManager>();

        internal static PlayerLineConnector GetTeamLineConnector(int teamIndex) => teamIndex == 0
            ? Object.FindObjectOfType<HelpersCollection>()._teamBlueLineConnector
            : Object.FindObjectOfType<HelpersCollection>()._teamRedLineConnector;
    }
}