using UnityEngine;

namespace Examples.Game.Scripts.Battle.interfaces
{
    /// <summary>
    /// Interface for external ball control, for example for <c>Game Manager</c> to use.
    /// </summary>
    public interface IBallControl
    {
        int currentTeamIndex { get; }
        void teleportBall(Vector2 position, int teamIndex); // We need to know onto which team's side we are landing!
        void moveBall(Vector2 direction, float speed);
        void showBall();
        void hideBall();
    }
}