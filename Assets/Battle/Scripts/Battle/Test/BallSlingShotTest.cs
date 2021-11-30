using Examples.Game.Scripts.Battle.Ball;
using Examples.Game.Scripts.Battle.interfaces;
using Examples.Game.Scripts.Battle.Player;
using Examples.Game.Scripts.Battle.SlingShot;
using Photon.Pun;
using System.Linq;
using UnityEngine;

namespace Examples.Game.Scripts.Battle.Test
{
    public class BallSlingShotTest : MonoBehaviour
    {
        public KeyCode controlKey = KeyCode.F3;

        private void Awake()
        {
            if (!PhotonNetwork.IsMasterClient)
            {
                gameObject.SetActive(false);
            }
        }

        private void Update()
        {
            if (Input.GetKeyDown(controlKey))
            {
                BallSlingShot.startTheBall();
                gameObject.SetActive(false);
            }
        }
    }
}