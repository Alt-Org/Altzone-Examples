using Examples2.Scripts.Battle.interfaces;
using Prg.Scripts.Common.PubSub;
using UnityEngine;
using UnityEngine.Assertions;

namespace Examples2.Scripts.Battle.Room
{
    /// <summary>
    /// Collects local scores and synchronizes them over network.
    /// </summary>
    public class ScoreManager : MonoBehaviour, IScoreManager
    {
        private void OnEnable()
        {
            Debug.Log($"OnEnable");
            this.Subscribe<ScoreEvent>(OnScoreEvent);
        }

        private void OnDisable()
        {
            this.Unsubscribe();
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
            Debug.Log($"AddScore {scoreType} {scoreAmount}");
            Assert.IsTrue(scoreAmount > 0, "scoreAmount > 0");
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
        }
    }
}