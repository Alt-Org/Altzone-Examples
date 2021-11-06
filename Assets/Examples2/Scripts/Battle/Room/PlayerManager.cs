using System.Collections;
using Examples2.Scripts.Battle.Factory;
using Examples2.Scripts.Battle.interfaces;
using Examples2.Scripts.Battle.Photon;
using Photon.Pun;
using Prg.Scripts.Common.Photon;
using UnityEngine;
using UnityEngine.Assertions;

namespace Examples2.Scripts.Battle.Room
{
    /// <summary>
    /// Manages players initial creation and gameplay start.
    /// </summary>
    internal class PlayerManager : MonoBehaviour, IPlayerManager
    {
        private const int MsgCountdown = PhotonEventDispatcher.eventCodeBase + 3;

        [SerializeField] private GameObject _playerPrefab;

        private PhotonEventDispatcher _photonEventDispatcher;
        private ICountdownManager _countdownManager;
        private IPlayerLineConnector _playerLineConnector;

        private void Awake()
        {
            Debug.Log("Awake");
            _photonEventDispatcher = PhotonEventDispatcher.Get();
            _photonEventDispatcher.registerEventListener(MsgCountdown, data => { OnCountdown(data.CustomData); });
        }

        private void OnEnable()
        {
            var player = PhotonNetwork.LocalPlayer;
            if (!PhotonBattle.IsRealPlayer(player))
            {
                Debug.Log($"OnEnable SKIP player {player.GetDebugLabel()}");
                return;
            }
            var playerPos = PhotonBattle.GetPlayerPos(player);
            var x = playerPos == 1 || playerPos == 2 ? 2.5f : -2.5f;
            var y = playerPos == 1 || playerPos == 3 ? 4.25f : -4.25f;
            var instantiationPosition = new Vector3(x, y);
            Debug.Log($"OnEnable create player {player.GetDebugLabel()} @ {instantiationPosition} from {_playerPrefab.name}");
            PhotonNetwork.Instantiate(_playerPrefab.name, instantiationPosition, Quaternion.identity);
        }

        #region Photon Events

        private void OnCountdown(object data)
        {
            var payload = (int[])data;
            Assert.AreEqual(payload.Length, 3, "Invalid message length");
            Assert.AreEqual(MsgCountdown, payload[0], "Invalid message id");
            var curValue = payload[1];
            var maxValue = payload[2];
            if (curValue == maxValue)
            {
                _countdownManager.StartCountdown(curValue);
            }
            else if (curValue >= 0)
            {
                _countdownManager.ShowCountdown(curValue);
            }
            else
            {
                _countdownManager.HideCountdown();
                _countdownManager = null;
                _playerLineConnector.Hide();
                _playerLineConnector = null;
            }
        }

        private void SendCountdown(int curValue, int maxValue)
        {
            var payload = new[] { MsgCountdown, curValue, maxValue };
            _photonEventDispatcher.RaiseEvent(MsgCountdown, payload);
        }

        #endregion

        private IEnumerator DoCountdown(int startValue)
        {
            var curValue = startValue;
            SendCountdown(curValue, startValue);
            var delay = new WaitForSeconds(1f);
            for (;;)
            {
                yield return delay;
                SendCountdown(--curValue, startValue);
                if (curValue < 0)
                {
                    yield break;
                }
            }
        }

        #region IPlayerManager

        void IPlayerManager.StartCountdown()
        {
            var player = PhotonNetwork.LocalPlayer;
            Debug.Log($"StartCountdown {player.GetDebugLabel()}");
            _countdownManager = Context.GetCountdownManager;
            StartCoroutine(DoCountdown(3));
            // Testing stuff here!
            var playerActor = Context.GetPlayer(PhotonBattle.GetPlayerPos(player));
            _playerLineConnector = Context.GetTeamLineConnector(playerActor.TeamIndex);
            _playerLineConnector.Connect(playerActor);
        }

        void IPlayerManager.StartGameplay()
        {
            Debug.Log($"StartGameplay {PhotonNetwork.LocalPlayer.GetDebugLabel()}");
        }

        void IPlayerManager.StopGameplay()
        {
            Debug.Log($"StopGameplay {PhotonNetwork.LocalPlayer.GetDebugLabel()}");
        }

        #endregion
    }
}