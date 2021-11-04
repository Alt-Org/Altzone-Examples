using System;
using UnityEngine;

namespace Examples2.Scripts.Battle.interfaces
{
    internal enum TeamColor
    {
        None,
        Red,
        Blue
    }

    internal enum BallColor : byte
    {
        Placeholder = 0,
        NoTeam = 1,
        RedTeam = 2,
        BlueTeam = 3,
        Ghosted = 4,
        Hidden = 5
    }

    internal interface IBall
    {
        IBallCollision BallCollision { get; }
        void StopMoving();
        void StartMoving(Vector2 position, Vector2 velocity);
        void SetColor(BallColor ballColor);
    }

    internal interface IBallCollision
    {
        Action<GameObject> OnHeadCollision { get; set; }
        Action<GameObject> OnShieldCollision { get; set; }
        Action<GameObject> OnBrickCollision { get; set; }
        Action<GameObject> OnWallCollision { get; set; }
        Action<GameObject> OnEnterTeamArea { get; set; }
        Action<GameObject> OnExitTeamArea { get; set; }
    }
}