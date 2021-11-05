using UnityEngine;

namespace Examples2.Scripts.Battle.interfaces
{
    /// <summary>
    /// Interface to move player towards given position with given speed.
    /// </summary>
    public interface IMovablePlayer
    {
        Transform Transform { get; }
        float Speed { get; set; }
        void MoveTo(Vector2 position);
    }
}