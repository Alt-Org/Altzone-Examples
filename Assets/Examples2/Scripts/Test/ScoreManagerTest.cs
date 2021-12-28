using Examples2.Scripts.Battle.Factory;
using Examples2.Scripts.Battle.interfaces;
using Examples2.Scripts.Battle.Room;
using Prg.Scripts.Common.PubSub;
using UnityEngine;

namespace Examples2.Scripts.Test
{
    public class ScoreManagerTest : MonoBehaviour
    {
        public enum ScoreType
        {
            BadEnum = 0,
            BlueHead = 1,
            RedHead = 2,
            BlueWall = 3,
            RedWall = 4,
        }

        [Header("Debug Only")] public bool _addScore;
        public bool _publishScore;

        [Header("Score")] public ScoreType _scoreType;
        [Min(0)] public int _scoreAmount;

        private IScoreManager _scoreManager;

        private IScoreManager ScoreManager => _scoreManager ?? (_scoreManager = Context.GetScoreManager);

        private void Update()
        {
            if (_addScore)
            {
                _addScore = false;
                var scoreType = (Battle.interfaces.ScoreType)_scoreType;
                ScoreManager.AddScore(scoreType, _scoreAmount);
                return;
            }
            if (_publishScore)
            {
                _publishScore = false;
                var scoreType = (Battle.interfaces.ScoreType)_scoreType;
                this.Publish(new ScoreManager.ScoreEvent(scoreType, _scoreAmount));
            }
        }
    }
}