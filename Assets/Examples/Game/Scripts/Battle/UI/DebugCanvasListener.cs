﻿using Examples.Config.Scripts;
using Examples.Game.Scripts.Battle.Player;
using Examples.Game.Scripts.Battle.Room;
using Examples.Game.Scripts.Battle.Test;
using Photon.Pun;
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
        private const string scoreFormatLocal = "<color={4}>{0}({1})</color> h={2} w={3}";
        private const string scoreFormatVisitor = "<color={4}>{0}({1})</color> h={2} w={3}";

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
            if (PlayerActivator.homeTeamIndex == 0)
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
            Debug.Log($"OnTeamScoreEvent {data} homeTeamIndex={PlayerActivator.homeTeamIndex}");
            var score = data.score;
            var isHomeTeam = score.teamIndex == PlayerActivator.homeTeamIndex;
            var teamName = isHomeTeam ? _teamNameHome : _teamNameVisitor;
            var teamColor = isHomeTeam ? _teamColorHome : _teamColorVisitor;
            var text = isHomeTeam ? leftText : rightText;
            var isLocalTeam = score.teamIndex == PlayerActivator.localTeamIndex;
            var format = isLocalTeam ? scoreFormatLocal : scoreFormatVisitor;
            text.text = string.Format(format, teamName, score.teamIndex, score.headCollisionCount, score.wallCollisionCount, teamColor);
        }
    }
}