using UnityEngine;

namespace Examples2.Scripts.Battle.Room
{
    /// <summary>
    /// Automatic marker to identify <c>GameObject</c> with unique id.
    /// </summary>
    public class IdMarker : MonoBehaviour
    {
        private static int _idCounter;

        [SerializeField] private int id;

        public int Id
        {
            get => id;
            set => id = value;
        }

        private void Awake()
        {
            id = ++_idCounter;
        }
    }
}