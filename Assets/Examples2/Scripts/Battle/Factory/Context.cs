using System.Collections.Generic;
using System.Linq;
using Altzone.Scripts.Battle;
using Examples2.Scripts.Battle.Ball;
using Examples2.Scripts.Battle.interfaces;
using Examples2.Scripts.Battle.Players;
using Examples2.Scripts.Battle.Room;
using UnityEngine;

namespace Examples2.Scripts.Battle.Factory
{
    /// <summary>
    /// Helper class to find all actors and managers in the game in order to hide dependencies to actual <c>MonoBehaviour</c> implementations.
    /// </summary>
    /// <remarks>
    /// This class is can be sensitive to <c>script execution order</c>!
    /// </remarks>
    internal static class Context
    {
        internal static GameCamera GetGameCamera => Object.FindObjectOfType<GameCamera>();

        internal static IPlayerManager GetPlayerManager => Object.FindObjectOfType<PlayerManager>();

        internal static IBall GetBall => Object.FindObjectOfType<BallActor>();

        internal static List<IPlayerActor> GetPlayers => Object.FindObjectsOfType<PlayerActor>().Cast<IPlayerActor>().ToList();

        internal static IPlayerActor GetPlayer(int playerPos) =>
            Object.FindObjectsOfType<PlayerActor>().Cast<IPlayerActor>().First(x => x.PlayerPos == playerPos);

        internal static IPlayerActor FindPlayer(int playerPos) =>
            Object.FindObjectsOfType<PlayerActor>().Cast<IPlayerActor>().FirstOrDefault(x => x.PlayerPos == playerPos);

        internal static IBrickManager GetBrickManager => Object.FindObjectOfType<BrickManager>();

        internal static IPlayerLineConnector GetTeamLineConnector(int teamNumber) => teamNumber == PhotonBattle.TeamBlueValue
            ? Object.FindObjectOfType<HelpersCollection>()?.TeamBlueLineConnector
            : Object.FindObjectOfType<HelpersCollection>()?.TeamRedLineConnector;
    }
}