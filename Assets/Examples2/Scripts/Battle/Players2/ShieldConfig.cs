using UnityEngine;
using UnityEngine.Assertions;

namespace Examples2.Scripts.Battle.Players2
{
    public class ShieldConfig : MonoBehaviour
    {
        private const string Tooltip = "Leave Shields empty for auto config";

        [SerializeField, Tooltip(Tooltip)] private Transform[] _shields;
        [SerializeField] public ParticleSystem _particle;

        public Transform[] Shields => _shields;

        private void Awake()
        {
            Assert.IsTrue(_shields.Length > 0, "_shields.Length > 0");
            var childCount = _shields.Length;
            for (int i = 0; i < childCount; i++)
            {
                var child = _shields[i];
                child.gameObject.SetActive(i == 0);
                child.localPosition = Vector3.zero;
            }
        }
    }
}
