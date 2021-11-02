using System;
using UnityEngine;

namespace Examples2.Scripts.Battle.interfaces
{
    internal enum TeamColor
    {
        None,
        Red,
        Blue,
    }

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
        IBallCollision ballCollision { get; }
        void stopMoving();
        void startMoving(Vector2 position, Vector2 velocity);
        void setColor(BallColor ballColor);
    }

    internal interface IBallCollision
    {
        Action<GameObject> onHeadCollision { get; set; }
        Action<GameObject> onShieldCollision { get; set; }
        Action<GameObject> onBrickCollision { get; set; }
        Action<GameObject> onWallCollision { get; set; }
        Action<TeamColor> onEnterTeamArea { get; set; }
        Action<TeamColor> onExitTeamArea { get; set; }
    }
}