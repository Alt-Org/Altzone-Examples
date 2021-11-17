using Photon.Pun;
using Prg.Scripts.Common.Photon;
using Prg.Scripts.Common.PubSub;
using System;
using Altzone.Scripts.Battle;
using Examples.Config.Scripts;
using Examples.Game.Scripts.Battle.Player;
using UnityEngine;

namespace Examples.Game.Scripts.Battle.Room
{
    /// <summary>
    /// Data holder class for team score.
    /// </summary>
    [Serializable]
    public class TeamScore
    {
        public int _teamIndex;
        public int _headCollisionCount;
        public int _wallCollisionCount;

        public byte[] ToBytes()
        {
            return new[] { (byte)_teamIndex, (byte)_headCollisionCount, (byte)_wallCollisionCount };
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
            return $"team: {_teamIndex}, headCollision: {_headCollisionCount}, wallCollision: {_wallCollisionCount}";
        }
    }

    /// <summary>
    /// Optional score manager for the room that synchronizes game score over network.
    /// </summary>
    public class ScoreManager : MonoBehaviour
    {
        private const int MsgSetTeamScore = PhotonEventDispatcher.eventCodeBase + 6;

        private static ScoreManager Get()
        {
            if (_instance == null)
            {
                _instance = FindObjectOfType<ScoreManager>();
            }
            return _instance;
        }

        private static ScoreManager _instance;

        [SerializeField] private TeamScore[] _scores;

        private PhotonEventDispatcher _photonEventDispatcher;

        private void Awake()
        {
            _scores = new[]
            {
                new TeamScore { _teamIndex = 0 },
                new TeamScore { _teamIndex = 1 },
            };
            _photonEventDispatcher = PhotonEventDispatcher.Get();
            _photonEventDispatcher.registerEventListener(MsgSetTeamScore, data => { ONSetTeamScore(data.CustomData); });
        }

        private void OnDestroy()
        {
            _instance = null;
        }

        private void OnEnable()
        {
            this.Subscribe<TeamScoreEvent>(ONTeamScoreEvent);
            // Set initial state for scores
            SendTeamNames();
            this.Publish(new TeamScoreEvent(_scores[0]));
            this.Publish(new TeamScoreEvent(_scores[1]));
        }

        private void OnDisable()
        {
            this.Unsubscribe();
        }

        private void SendTeamNames()
        {
            var room = PhotonNetwork.CurrentRoom;
            string teamRedName;
            string teamBlueName;
            if (PlayerActivator.HomeTeamIndex == 0)
            {
                teamRedName = room.GetCustomProperty<string>(PhotonBattle.TeamRedKey);
                teamBlueName = room.GetCustomProperty<string>(PhotonBattle.TeamBlueKey);
            }
            else
            {
                teamRedName = room.GetCustomProperty<string>(PhotonBattle.TeamBlueKey);
                teamBlueName = room.GetCustomProperty<string>(PhotonBattle.TeamRedKey);
            }
            this.Publish(new TeamNameEvent(teamRedName, teamBlueName));
        }

        private void SendSetTeamScore(TeamScore score)
        {
            _photonEventDispatcher.RaiseEvent(MsgSetTeamScore, score.ToBytes());
        }

        private void ONSetTeamScore(object data)
        {
            TeamScore.FromBytes(data, out var teamIndex, out var headCollisionCount, out var wallCollisionCount);
            var score = _scores[teamIndex];
            // Update and publish new score
            score._headCollisionCount = headCollisionCount;
            score._wallCollisionCount = wallCollisionCount;
            this.Publish(new TeamScoreEvent(score));
        }

        private void ONTeamScoreEvent(TeamScoreEvent data)
        {
            var scoreNew = data.Score;
            var score = _scores[scoreNew._teamIndex];
            score._headCollisionCount = scoreNew._headCollisionCount;
            score._wallCollisionCount = scoreNew._wallCollisionCount;
        }

        private void _addHeadScore(int teamIndex)
        {
            var score = _scores[teamIndex];
            score._headCollisionCount += 1;
            // Send updated score to everybody
            SendSetTeamScore(score);
        }

        private void _addWallScore(int teamIndex)
        {
            var score = _scores[teamIndex];
            score._wallCollisionCount += 1;
            // Send updated score to everybody
            SendSetTeamScore(score);
        }

        public static void AddHeadScore(int teamIndex)
        {
            var manager = Get();
            if (PhotonNetwork.IsMasterClient && manager != null)
            {
                Get()._addHeadScore(teamIndex);
            }
        }

        public static void AddWallScore(GameObject gameObject)
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

        internal class TeamNameEvent
        {
            public readonly string TeamRedName;
            public readonly string TeamBlueName;

            public TeamNameEvent(string teamRedName, string teamBlueName)
            {
                TeamRedName = teamRedName;
                TeamBlueName = teamBlueName;
            }
        }

        internal class TeamScoreEvent
        {
            public readonly TeamScore Score;

            public TeamScoreEvent(TeamScore score)
            {
                this.Score = score;
            }

            public override string ToString()
            {
                return $"{nameof(Score)}: {Score}";
            }
        }
    }
}