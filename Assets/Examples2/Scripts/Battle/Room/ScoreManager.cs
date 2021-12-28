using Examples2.Scripts.Battle.interfaces;
using Photon.Pun;
using Prg.Scripts.Common.Photon;
using Prg.Scripts.Common.PubSub;
using UnityEngine;
using UnityEngine.Assertions;

namespace Examples2.Scripts.Battle.Room
{
    /// <summary>
    /// Collects local scores and synchronizes them over network.
    /// </summary>
    internal class ScoreManager : MonoBehaviour, IScoreManager
    {
        private readonly LocalScore _localScore = new LocalScore();
        private readonly NetworkScore _networkScore = new NetworkScore();

        private void OnEnable()
        {
            Debug.Log($"OnEnable");
            _localScore.OnEnable();
            _networkScore.OnEnable();
            _networkScore.LocalScore = _localScore;
            this.Subscribe<ScoreEvent>(OnScoreEvent);
        }

        private void OnDisable()
        {
            this.Unsubscribe();
            _localScore.OnDisable();
            _networkScore.OnDisable();
        }

        private void OnScoreEvent(ScoreEvent data)
        {
            Debug.Log($"OnScoreEvent {data}");
            ((IScoreManager)this).AddScore(data.ScoreType, data.ScoreAmount);
        }

        void IScoreManager.AddScore(ScoreType scoreType)
        {
            ((IScoreManager)this).AddScore(scoreType, 1);
        }

        void IScoreManager.AddScore(ScoreType scoreType, int scoreAmount)
        {
            if (!PhotonNetwork.IsMasterClient)
            {
                return;
            }
            Debug.Log($"AddScore {scoreType} {scoreAmount}");
            Assert.IsTrue(scoreAmount > 0, "scoreAmount > 0");
            _networkScore.AddScore(scoreType, scoreAmount);
        }

        internal class ScoreEvent
        {
            public readonly ScoreType ScoreType;
            public readonly int ScoreAmount;

            public ScoreEvent(ScoreType scoreType, int scoreAmount = 1)
            {
                ScoreType = scoreType;
                ScoreAmount = scoreAmount;
            }

            public override string ToString()
            {
                return $"Score: {ScoreType} : {ScoreAmount}";
            }
        }

        internal class GameScoreEvent
        {
            public int TeamBlueHeadScore => _teamBlueHeadScore;
            public int TeamBlueWallScore => _teamBlueWallScore;
            public int TeamRedHeadScore => _teamRedHeadScore;
            public int TeamRedWallScore => _teamRedWallScore;

            private int _teamBlueHeadScore;
            private int _teamBlueWallScore;
            private int _teamRedHeadScore;
            private int _teamRedWallScore;

            internal GameScoreEvent(int teamBlueHeadScore, int teamBlueWallScore, int teamRedHeadScore, int teamRedWallScore)
            {
                _teamBlueHeadScore = teamBlueHeadScore;
                _teamBlueWallScore = teamBlueWallScore;
                _teamRedHeadScore = teamRedHeadScore;
                _teamRedWallScore = teamRedWallScore;
            }

            internal void AddScore(ScoreType scoreType, int scoreAmount)
            {
                switch (scoreType)
                {
                    case ScoreType.BlueWall:
                        _teamBlueWallScore += scoreAmount;
                        break;
                    case ScoreType.BlueHead:
                        _teamBlueHeadScore += scoreAmount;
                        break;
                    case ScoreType.RedWall:
                        _teamRedWallScore += scoreAmount;
                        break;
                    case ScoreType.RedHead:
                        _teamRedHeadScore += scoreAmount;
                        break;
                }
            }

            public override string ToString()
            {
                return $"BH: {TeamBlueHeadScore}, BW: {TeamBlueWallScore}, RH: {TeamRedHeadScore}, RW: {TeamRedWallScore}";
            }
        }

        private class LocalScore
        {
            private readonly GameScoreEvent _currentScore =
                new GameScoreEvent(0, 0, 0, 0);

            public void OnEnable()
            {
                // NOP.
            }

            public void OnDisable()
            {
                // NOP.
            }

            public void AddScore(ScoreType scoreType, int scoreAmount)
            {
                _currentScore.AddScore(scoreType, scoreAmount);
                this.Publish(_currentScore);
            }
        }

        private class NetworkScore
        {
            private const int MsgSendScore = PhotonEventDispatcher.eventCodeBase + 4;

            public LocalScore LocalScore { get; set; }

            private PhotonEventDispatcher _photonEventDispatcher;

            public void OnEnable()
            {
                if (_photonEventDispatcher == null)
                {
                    _photonEventDispatcher = PhotonEventDispatcher.Get();
                    _photonEventDispatcher.registerEventListener(MsgSendScore, data => { OnSendScore(data.CustomData); });
                }
            }

            public void OnDisable()
            {
                // No need to unregister anything.
            }

            #region Photon Events

            private void OnSendScore(object data)
            {
                var payload = (byte[])data;
                Assert.AreEqual(3, payload.Length, "Invalid message length");
                Assert.AreEqual((byte)MsgSendScore, payload[0], "Invalid message id");
                var scoreType = (ScoreType)payload[1];
                var scoreAmount = (int)payload[1];
                LocalScore.AddScore(scoreType, scoreAmount);
            }

            public void AddScore(ScoreType scoreType, int scoreAmount)
            {
                var payload = new[] { (byte)MsgSendScore, (byte)scoreType, (byte)scoreAmount };
                _photonEventDispatcher.RaiseEvent(MsgSendScore, payload);
            }

            #endregion
        }
    }
}