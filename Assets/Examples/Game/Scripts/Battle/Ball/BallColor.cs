using Examples.Config.Scripts;
using Photon.Pun;
using Prg.Scripts.Common.Photon;
using Prg.Scripts.Common.PubSub;
using UnityEngine;

namespace Examples.Game.Scripts.Battle.Ball
{
    public class BallColor : MonoBehaviour
    {
        private const int msgSetBallColor = PhotonEventDispatcher.eventCodeBase + 4;

        private const int neutralColorIndex = 0;
        private const int upperTeamColorIndex = 1;
        private const int lowerTeamColorIndex = 2;

        [Header("Settings"), SerializeField] private SpriteRenderer _sprite;
        [SerializeField] private Color neutralColor;
        [SerializeField] private Color upperTeamColor;
        [SerializeField] private Color lowerTeamColor;

        private PhotonEventDispatcher photonEventDispatcher;

        private void Awake()
        {
            Debug.Log("Awake");
            photonEventDispatcher = PhotonEventDispatcher.Get();
            photonEventDispatcher.registerEventListener(msgSetBallColor, data => { onSetBallColor(data.CustomData); });
        }

        private void sendSetBallColor(int colorIndex)
        {
            photonEventDispatcher.RaiseEvent(msgSetBallColor, colorIndex);
        }

        private void onSetBallColor(object data)
        {
            var colorIndex = (int)data;
            switch (colorIndex)
            {
                case lowerTeamColorIndex:
                    _sprite.color = lowerTeamColor;
                    break;
                case upperTeamColorIndex:
                    _sprite.color = upperTeamColor;
                    break;
                default:
                    _sprite.color = neutralColor;
                    break;
            }
        }

        private void OnEnable()
        {
            var player = PhotonNetwork.LocalPlayer;
            PhotonBattle.getPlayerProperties(player, out var playerPos, out var teamIndex);
            if (teamIndex == 1)
            {
                // c# swap via deconstruction
                (upperTeamColor, lowerTeamColor) = (lowerTeamColor, upperTeamColor);
            }
            this.Subscribe<BallActor.ActiveTeamEvent>(onActiveTeamEvent);
        }

        private void OnDisable()
        {
            this.Unsubscribe();
        }

        private void onActiveTeamEvent(BallActor.ActiveTeamEvent data)
        {
            switch (data.newTeamIndex)
            {
                case 0:
                    sendSetBallColor(lowerTeamColorIndex);
                    break;
                case 1:
                    sendSetBallColor(upperTeamColorIndex);
                    break;
                default:
                    sendSetBallColor(neutralColorIndex);
                    break;
            }
        }
    }
}