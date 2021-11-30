using Battle.Scripts.Battle.SlingShot;
using Photon.Pun;
using UnityEngine;

namespace Battle.Scripts.Battle.Test
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