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

    internal enum BallColor : byte
    {
        Placeholder = 0,
        NoTeam = 1,
        RedTeam = 2,
        BlueTeam = 3,
        Ghosted = 4,
        Hidden = 5,
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
        Action<GameObject> onEnterTeamArea { get; set; }
        Action<GameObject> onExitTeamArea { get; set; }
    }
}