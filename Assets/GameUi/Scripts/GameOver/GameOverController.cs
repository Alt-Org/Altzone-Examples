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
                var teamBlueWinning = PhotonWrapper.GetRoomProperty(PhotonBattle.TeamBlueKey, 0);
                if (teamBlueWinning != 0)
                {
                    _view.WinnerInfo1 = RichText.Blue("Team BLUE");
                    break;
                }
                var teamRedWinning = PhotonWrapper.GetRoomProperty(PhotonBattle.TeamRedKey, 0);
                if (teamRedWinning != 0)
                {
                    _view.WinnerInfo1 = RichText.Red("Team RED");
                    break;
                }
                if (Time.time > timeOutTime)
                {
                    _view.WinnerInfo1 = RichText.Yellow("UNKNOWN RESULT");
                    _view.WinnerInfo2 = "No scores found";
                    break;
                }
                yield return null;
            }
            _view.ContinueButton.interactable = true;
        }
    }
}