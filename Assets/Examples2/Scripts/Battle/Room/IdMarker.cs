using UnityEngine;

namespace Examples2.Scripts.Battle.Room
{
    /// <summary>
    /// Automatic marker to identify <c>GameObject</c> with unique id.
    /// </summary>
    public class IdMarker : MonoBehaviour
    {
        private static int idCounter;

        [SerializeField] private int _id;

        public int Id
        {
            get => _id;
            set => _id = value;
        }

        private void Awake()
        {
            _id = ++idCounter;
        }

        public override string ToString()
        {
            return $"{gameObject.name}#{_id}";
        }
    }
}