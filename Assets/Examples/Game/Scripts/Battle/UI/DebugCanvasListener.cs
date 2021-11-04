using Examples.Game.Scripts.Battle.Player;
using Examples.Game.Scripts.Battle.Room;
using Examples.Game.Scripts.Battle.Test;
using Prg.Scripts.Common.PubSub;
using UnityEngine;
using UnityEngine.UI;

namespace Examples.Game.Scripts.Battle.UI
{
    /// <summary>
    /// Simple temporary UI for debugging.
    /// </summary>
    public class DebugCanvasListener : MonoBehaviour
    {
        private const string ScoreFormatLocal = "<color={4}>{0}({1})</color> h={2} w={3}";
        private const string ScoreFormatVisitor = "<color={4}>{0}({1})</color> h={2} w={3}";

        public GameObject roomStartPanel;
        public Text titleText;
        public GameObject scorePanel;
        public Text leftText;
        public Text rightText;
        public CountdownText countdown;

        private string _teamNameHome;
        private string _teamNameVisitor;
        private string _teamColorHome;
        private string _teamColorVisitor;

        private void OnEnable()
        {
            roomStartPanel.SetActive(false);
            scorePanel.SetActive(false);
            leftText.text = "";
            rightText.text = "";
            this.Subscribe<ScoreManager.TeamNameEvent>(OnTeamNameEvent);
            this.Subscribe<ScoreManager.TeamScoreEvent>(OnTeamScoreEvent);
            this.Subscribe<GameStartPlayingTest.CountdownEvent>(OnCountdownEvent);
        }

        private void OnDisable()
        {
            leftText.text = "";
            rightText.text = "";
            this.Unsubscribe();
        }

        private void OnCountdownEvent(GameStartPlayingTest.CountdownEvent data)
        {
            Debug.Log($"OnCountdownEvent {data}");
            if (data.maxCountdownValue == data.curCountdownValue)
            {
                roomStartPanel.SetActive(true);
                titleText.text = "Wait for game start:";
            }
            countdown.setCountdownValue(data.curCountdownValue);
            if (data.curCountdownValue <= 0)
            {
                this.ExecuteAsCoroutine(new WaitForSeconds(0.67f), () =>
                {
                    roomStartPanel.SetActive(false);
                    scorePanel.SetActive(true);
                });
            }
        }

        private void OnTeamNameEvent(ScoreManager.TeamNameEvent data)
        {
            _teamNameVisitor = data.TeamRedName;
            _teamNameHome = data.TeamBlueName;
            if (PlayerActivator.HomeTeamIndex == 0)
            {
                _teamColorHome = "yellow";
                _teamColorVisitor = "white";
            }
            else
            {
                _teamColorHome = "white";
                _teamColorVisitor = "yellow";
            }
        }

        private void OnTeamScoreEvent(ScoreManager.TeamScoreEvent data)
        {
            Debug.Log($"OnTeamScoreEvent {data} homeTeamIndex={PlayerActivator.HomeTeamIndex}");
            var score = data.Score;
            var isHomeTeam = score._teamIndex == PlayerActivator.HomeTeamIndex;
            var teamName = isHomeTeam ? _teamNameHome : _teamNameVisitor;
            var teamColor = isHomeTeam ? _teamColorHome : _teamColorVisitor;
            var text = isHomeTeam ? leftText : rightText;
            var isLocalTeam = score._teamIndex == PlayerActivator.LocalTeamIndex;
            var format = isLocalTeam ? ScoreFormatLocal : ScoreFormatVisitor;
            text.text = string.Format(format, teamName, score._teamIndex, score._headCollisionCount, score._wallCollisionCount, teamColor);
        }
    }
}