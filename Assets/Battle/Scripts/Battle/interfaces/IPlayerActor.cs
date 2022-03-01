using UnityEngine;

namespace Battle.Scripts.Battle.interfaces
{
    public interface IPlayerActor
    {
        Transform Transform { get; }
        int PlayerPos { get; }
        int TeamNumber { get; }
        IPlayerActor TeamMate { get; }
        void HeadCollision();
        void ShieldCollision(Vector2 contactPoint);
        void SetNormalMode();
        void SetFrozenMode();
        void SetGhostedMode();
    }
}