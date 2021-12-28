using Altzone.Scripts.Battle;
using Examples2.Scripts.Battle.Factory;
using Examples2.Scripts.Battle.interfaces;
using Photon.Pun;
using Prg.Scripts.Common.PubSub;
using UnityEngine;
using UnityEngine.Assertions;

namespace Examples2.Scripts.Battle.Room
{
    /// <summary>
    /// Manages room gameplay state from start to game over.
    /// </summary>
    internal class RoomManager : MonoBehaviour
    {
        [Header("Live Data"), SerializeField] private int _requiredActorCount;
        [SerializeField] private int _currentActorCount;
        [SerializeField] private bool _isWaitForActors;
        [SerializeField] private bool _isWaitForCountdown;

        private IPlayerManager _playerManager;

        private void Awake()
        {
            _requiredActorCount = 1 + PhotonBattle.CountRealPlayers();
            _currentActorCount = 0;
            _isWaitForActors = true;
            _isWaitForCountdown = false;
            Debug.Log($"Awake required {_requiredActorCount} master {PhotonNetwork.IsMasterClient}");
            this.Subscribe<ActorReportEvent>(OnActorReportEvent);
            this.Subscribe<ScoreManager.GameScoreEvent>(OnGameScoreEvent);
        }

        private void OnDestroy()
        {
            this.Unsubscribe();
        }

        private void OnActorReportEvent(ActorReportEvent data)
        {
            _currentActorCount += 1;
            Debug.Log(
                $"OnActorReportEvent component {data.ComponentTypeId} required {_requiredActorCount} current {_currentActorCount} master {PhotonNetwork.IsMasterClient}");
            Assert.IsTrue(_currentActorCount <= _requiredActorCount);
            if (_currentActorCount == _requiredActorCount)
            {
                Assert.IsTrue(_isWaitForActors);
                _isWaitForActors = false;
                Assert.IsFalse(_isWaitForCountdown);
                _isWaitForCountdown = true;
                _playerManager = Context.GetPlayerManager;
                _playerManager.StartCountdown(OnCountdownFinished);
            }
        }

        private void OnGameScoreEvent(ScoreManager.GameScoreEvent data)
        {
            Debug.Log($"OnGameScoreEvent {data}");
        }

        private void OnCountdownFinished()
        {
            Debug.Log($"OnCountdownFinished master {PhotonNetwork.IsMasterClient}");
            Assert.IsTrue(_isWaitForCountdown);
            _isWaitForCountdown = false;
            _playerManager.StartGameplay();
        }

        internal class ActorReportEvent
        {
            public readonly int ComponentTypeId;

            public ActorReportEvent(int componentTypeId)
            {
                ComponentTypeId = componentTypeId;
            }
        }
    }
}