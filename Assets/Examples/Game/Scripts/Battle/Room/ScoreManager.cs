using Photon.Pun;
using Prg.Scripts.Common.Photon;
using Prg.Scripts.Common.PubSub;
using System;
using UnityEngine;

namespace Examples.Game.Scripts.Battle.Room
{
    /// <summary>
    /// Data holder class for team score.
    /// </summary>
    [Serializable]
    public class TeamScore
    {
        public int teamIndex;
        public int headCollisionCount;
        public int wallCollisionCount;

        public byte[] ToBytes()
        {
            return new[] { (byte)teamIndex, (byte)headCollisionCount, (byte)wallCollisionCount };
        }

        public static void FromBytes(object data, out int teamIndex, out int headCollisionCount, out int wallCollisionCount)
        {
            var payload = (byte[])data;
            teamIndex = payload[0];
            headCollisionCount = payload[1];
            wallCollisionCount = payload[2];
        }

        public override string ToString()
        {
            return $"team: {teamIndex}, headCollision: {headCollisionCount}, wallCollision: {wallCollisionCount}";
        }
    }

    /// <summary>
    /// Optional score manager for the room that synchronizes game score over network.
    /// </summary>
    public class ScoreManager : MonoBehaviour
    {
        private const int msgSetTeamScore = PhotonEventDispatcher.eventCodeBase + 6;

        private static ScoreManager Get()
        {
            if (_Instance == null)
            {
                _Instance = FindObjectOfType<ScoreManager>();
            }
            return _Instance;
        }

        private static ScoreManager _Instance;


        [SerializeField] private TeamScore[] scores;

        private PhotonEventDispatcher photonEventDispatcher;

        private void Awake()
        {
            scores = new[]
            {
                new TeamScore { teamIndex = 0 },
                new TeamScore { teamIndex = 1 },
            };
            photonEventDispatcher = PhotonEventDispatcher.Get();
            photonEventDispatcher.registerEventListener(msgSetTeamScore, data => { onSetTeamScore(data.CustomData); });
        }

        private void OnEnable()
        {
            this.Subscribe<TeamScoreEvent>(onTeamScoreEvent);
            // Set initial state for scores
            this.Publish(new TeamScoreEvent(scores[0]));
            this.Publish(new TeamScoreEvent(scores[1]));
        }

        private void OnDisable()
        {
            this.Unsubscribe();
        }

        private void sendSetTeamScore(TeamScore score)
        {
            photonEventDispatcher.RaiseEvent(msgSetTeamScore, score.ToBytes());
        }

        private void onSetTeamScore(object data)
        {
            TeamScore.FromBytes(data, out var _teamIndex, out var _headCollisionCount, out var _wallCollisionCount);
            var score = scores[_teamIndex];
            // Update and publish new score
            score.headCollisionCount = _headCollisionCount;
            score.wallCollisionCount = _wallCollisionCount;
            this.Publish(new TeamScoreEvent(score));
        }

        private void onTeamScoreEvent(TeamScoreEvent data)
        {
            var scoreNew = data.score;
            var score = scores[scoreNew.teamIndex];
            score.headCollisionCount = scoreNew.headCollisionCount;
            score.wallCollisionCount = scoreNew.wallCollisionCount;
        }

        private void _addHeadScore(int teamIndex)
        {
            var score = scores[teamIndex];
            score.headCollisionCount += 1;
            // Send updated score to everybody
            sendSetTeamScore(score);
        }

        private void _addWallScore(int teamIndex)
        {
            var score = scores[teamIndex];
            score.wallCollisionCount += 1;
            // Send updated score to everybody
            sendSetTeamScore(score);
        }

        public static void addHeadScore(int teamIndex)
        {
            var manager = Get();
            if (PhotonNetwork.IsMasterClient && manager != null)
            {
                Get()._addHeadScore(teamIndex);
            }
        }

        public static void addWallScore(GameObject gameObject)
        {
            var manager = Get();
            if (PhotonNetwork.IsMasterClient && manager != null)
            {
                if (gameObject.CompareTag(UnityConstants.Tags.BotSide))
                {
                    Get()._addWallScore(0);
                }
                else if (gameObject.CompareTag(UnityConstants.Tags.TopSide))
                {
                    Get()._addWallScore(1);
                }
            }
        }

        public class TeamScoreEvent
        {
            public readonly TeamScore score;

            public TeamScoreEvent(TeamScore score)
            {
                this.score = score;
            }

            public override string ToString()
            {
                return $"{nameof(score)}: {score}";
            }
        }
    }
}