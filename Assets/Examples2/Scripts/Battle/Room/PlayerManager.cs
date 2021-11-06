using System;
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
        private Action _countdownFinished;
        private IPlayerLineConnector _playerLineConnector;
        private PlayerLineResult _nearest;

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
            Debug.Log($"OnCountdown {curValue}/{maxValue}");
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
                _nearest = _playerLineConnector.GetNearest();
                _playerLineConnector.Hide();
                _playerLineConnector = null;
                _countdownFinished?.Invoke();
                _countdownFinished = null;
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

        void IPlayerManager.StartCountdown(Action countdownFinished)
        {
            var player = PhotonNetwork.LocalPlayer;
            Debug.Log($"StartCountdown {player.GetDebugLabel()} master {PhotonNetwork.IsMasterClient}");
            _countdownManager = Context.GetCountdownManager;
            _countdownFinished = countdownFinished;
            if (PhotonNetwork.IsMasterClient)
            {
                StartCoroutine(DoCountdown(3));
            }
            var playerActor = Context.GetPlayer(PhotonBattle.GetPlayerPos(player));
            _playerLineConnector = Context.GetTeamLineConnector(playerActor.TeamIndex);
            _playerLineConnector.Connect(playerActor);
        }

        void IPlayerManager.StartGameplay()
        {
            Debug.Log(
                $"StartGameplay nearest {_nearest.PlayerActor.Transform.name} distY {_nearest.DistanceY:F1} master {PhotonNetwork.IsMasterClient}");
            foreach (var playerActor in Context.GetPlayers)
            {
                var actorTransform = playerActor.Transform;
                var actorPosition = actorTransform.position;
                var dist = Mathf.Abs((actorPosition - Vector3.zero).magnitude);
                Debug.Log($"{actorTransform.name} x={actorPosition.x:F1} y={actorPosition.y:F1} dist={dist:F1}");
            }
            if (PhotonNetwork.IsMasterClient)
            {
                //_nearest.PlayerActor.Transform.gameObject.SetActive(false);
                var ball = Context.GetBall;
                ball.SetColor(BallColor.NoTeam);
                var position = _nearest.PlayerActor.Transform.position;
                ball.StartMoving(position, _nearest.Force);
            }
        }

        void IPlayerManager.StopGameplay()
        {
            Debug.Log($"StopGameplay {PhotonNetwork.LocalPlayer.GetDebugLabel()}");
        }

        #endregion
    }
}