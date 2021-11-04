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
        NoTeam = 0,
        RedTeam = 1,
        BlueTeam = 2,
        Ghosted = 3,
        Hidden = 4
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