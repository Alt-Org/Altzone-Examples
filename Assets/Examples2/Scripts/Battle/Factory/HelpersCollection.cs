using Examples2.Scripts.Battle.PlayerConnect;
using UnityEngine;
using UnityEngine.Assertions;

namespace Examples2.Scripts.Battle.Factory
{
    internal class HelpersCollection : MonoBehaviour
    {
        [SerializeField] private PlayerLineConnector _teamRedLineConnector;
        [SerializeField] private PlayerLineConnector _teamBlueLineConnector;

        public PlayerLineConnector TeamRedLineConnector => _teamRedLineConnector;
        public PlayerLineConnector TeamBlueLineConnector => _teamBlueLineConnector;
    }
}