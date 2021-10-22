using Examples.Config.Scripts;
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
        private static readonly string[] teamName = { "Home", "Visitor" };

        public GameObject roomStartPanel;
        public Text titleText;
        public Text countdownText;
        public GameObject scorePanel;
        public Text leftText;
        public Text rightText;
        public int homeTeamIndex;

        private void OnEnable()
        {
            roomStartPanel.SetActive(false);
            scorePanel.SetActive(false);
            leftText.text = "";
            rightText.text = "";
            homeTeamIndex = 0;
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
            countdownText.text = data.curCountdownValue.ToString("N0");
            if (data.curCountdownValue <= 0)
            {
                this.executeAsCoroutine(new WaitForSeconds(0.67f), () =>
                {
                    roomStartPanel.SetActive(false);
                    activateScores();
                });
            }
        }

        private void activateScores()
        {
            var player = PhotonNetwork.LocalPlayer;
            PhotonBattle.getPlayerProperties(player, out _, out var teamIndex);
            var features = RuntimeGameConfig.Get().features;
            if (features.isLocalPLayerOnTeamBlue)
            {
                if (teamIndex == 1)
                {
                    homeTeamIndex = 1;
                    // c# swap via deconstruction
                    (leftText, rightText) = (rightText, leftText);
                }
            }
            scorePanel.SetActive(true);
        }

        private void OnTeamScoreEvent(ScoreManager.TeamScoreEvent data)
        {
            Debug.Log($"OnTeamScoreEvent {data}");
            var score = data.score;
            var text = score.teamIndex == homeTeamIndex ? leftText : rightText;
            text.text = $"<b>{teamName[score.teamIndex]}({score.teamIndex})</b> h={score.headCollisionCount} w={score.wallCollisionCount}";
        }
    }
}