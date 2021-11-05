using UnityEngine;

namespace Examples2.Scripts.Battle.interfaces
{
    public interface IPlayerActor
    {
        Transform Transform { get; }
        int PlayerPos { get; }
        int TeamIndex { get; }
        IPlayerActor TeamMate { get; }
    }
}