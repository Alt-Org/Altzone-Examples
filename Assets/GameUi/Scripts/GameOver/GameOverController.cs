using System.Collections;
using Altzone.Scripts.Battle;
using Prg.Scripts.Common.Photon;
using UnityEngine;

namespace GameUi.Scripts.GameOver
{
    public class GameOverController : MonoBehaviour
    {
        [SerializeField] private GameOverView _view;

        private void OnEnable()
        {
            _view.ContinueButton.interactable = false;
            StartCoroutine(WaitForWinner());
        }

        private IEnumerator WaitForWinner()
        {
            yield return null;
            while (PhotonWrapper.InRoom)
            {
                var teamBlueWinning = PhotonWrapper.GetRoomProperty(PhotonBattle.TeamBlueKey, 0);
                if (teamBlueWinning != 0)
                {
                    _view.WinnerInfo = RichText.Blue("Team BLUE");
                    break;
                }
                var teamRedWinning = PhotonWrapper.GetRoomProperty(PhotonBattle.TeamRedKey, 0);
                if (teamRedWinning != 0)
                {
                    _view.WinnerInfo = RichText.Red("Team RED");
                    break;
                }
                yield return null;
            }
            _view.ContinueButton.interactable = true;
        }
    }
}
