using UnityEngine;

namespace Examples2.Scripts.Battle.interfaces
{
    internal enum BallColor
    {
        NoTeam,
        RedTeam,
        BlueTeam,
        Ghosted,
        Hidden,
        Placeholder
    }

    internal interface IBall
    {
        void stopMoving();
        void startMoving(Vector2 position, Vector2 velocity);
        void setColor(BallColor ballColor);
    }
}