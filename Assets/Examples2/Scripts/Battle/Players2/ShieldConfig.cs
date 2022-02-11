using UnityEngine;

namespace Examples2.Scripts.Battle.Players2
{
    public class ShieldConfig : MonoBehaviour
    {
        [SerializeField] private Transform[] _shields;

        public Transform[] Shields => _shields;

        private void Awake()
        {
            var myTransform = GetComponent<Transform>();
            var childCount = myTransform.childCount;
            _shields = new Transform[childCount];
            for (int i = 0; i < childCount; i++)
            {
                _shields[i] = myTransform.GetChild(i);
            }
        }
    }
}