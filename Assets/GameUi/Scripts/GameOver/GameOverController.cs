using System.Collections;
using Altzone.Scripts.Battle;
using Prg.Scripts.Common.Photon;
using UnityEngine;

namespace GameUi.Scripts.GameOver
{
    public class GameOverController : MonoBehaviour
    {
        [SerializeField] private GameOverView _view;
        [SerializeField] private float _timeOutDelay;

        private void OnEnable()
        {
            if (!PhotonWrapper.InRoom)
            {
                _view.WinnerInfo1 = RichText.Yellow("NOBODY WINS");
                _view.WinnerInfo2 = string.Empty;
                return;
            }
            _view.ContinueButton.interactable = false;
            StartCoroutine(WaitForWinner());
        }

        private IEnumerator WaitForWinner()
        {
            yield return null;
            var timeOutTime = _timeOutDelay + Time.time;
            while (PhotonWrapper.InRoom)
            {
                if (Time.time > timeOutTime)
                {
                    _view.WinnerInfo1 = RichText.Yellow("UNKNOWN RESULT");
                    _view.WinnerInfo2 = "No scores found";
                    break;
                }
                var winnerTeam = PhotonWrapper.GetRoomProperty(PhotonBattle.TeamWinKey, -1);
                if (winnerTeam == -1)
                {
                    yield return null;
                    continue;
                }
                var blueScore = PhotonWrapper.GetRoomProperty(PhotonBattle.TeamBlueScoreKey, 0);
                var redScore = PhotonWrapper.GetRoomProperty(PhotonBattle.TeamRedScoreKey, 0);
                if (winnerTeam == PhotonBattle.TeamBlueValue)
                {
                    _view.WinnerInfo1 = RichText.Blue("Team BLUE");
                    _view.WinnerInfo2 = $"{blueScore} - {redScore}";
                }
                else if (winnerTeam == PhotonBattle.TeamRedValue)
                {
                    _view.WinnerInfo1 = RichText.Red("Team RED");
                    _view.WinnerInfo2 = $"{redScore} - {blueScore}";
                }
                else
                {
                    _view.WinnerInfo1 = RichText.Yellow("DRAW!");
                    _view.WinnerInfo2 = string.Empty;
                }
                break;
            }
            _view.ContinueButton.interactable = true;
        }
    }
}