using UnityEngine;

namespace Battle.Scripts.Battle.interfaces
{
    /// <summary>
    /// Interface to move player towards given position.
    /// </summary>
    public interface IMovablePlayer
    {
        void moveTo(Vector3 position);
    }
}