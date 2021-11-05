using Examples2.Scripts.Battle.interfaces;
using Examples2.Scripts.Battle.Photon;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;

namespace Examples2.Scripts.Battle.PlayerConnect
{
    public class PlayerLineConnector : MonoBehaviourPunCallbacks
    {
        [Header("Settings"), SerializeField] private LineRenderer _line;
        [SerializeField] private Vector3 _referencePoint;

        [Header("Live Data"), SerializeField] private Transform _transformA;
        [SerializeField] private Transform _transformB;
        [SerializeField] private float _distanceA;
        [SerializeField] private float _distanceB;

        private IPlayerActor _playerActorA;
        private IPlayerActor _playerActorB;
        private Vector3 _positionA;
        private Vector3 _positionB;

        private void Update()
        {
            _positionA = _transformA.position;
            _positionB = _transformB.position;
            _distanceA = Mathf.Abs(_referencePoint.y - _positionA.y);
            _distanceB = Mathf.Abs(_referencePoint.y - _positionB.y);
            // Index 1 is the arrow head!
            if (_distanceA < _distanceB)
            {
                _line.SetPosition(0, _positionB);
                _line.SetPosition(1, _positionA);
            }
            else
            {
                _line.SetPosition(0, _positionA);
                _line.SetPosition(1, _positionB);
            }
        }

        public void Connect(IPlayerActor playerActor)
        {
            Debug.Log($"Connect {playerActor}");
            _transformA = playerActor.Transform;
            _playerActorA = playerActor;
            if (playerActor.TeamMate != null)
            {
                _transformB = playerActor.TeamMate.Transform;
                _playerActorB = playerActor.TeamMate;
            }
            else
            {
                // We assume that we are positioned somewhere in team's gameplay area so that line renderer can work with one player.
                _transformB = GetComponent<Transform>();
                _playerActorB = null;
            }
            gameObject.SetActive(true);
        }

        public override void OnPlayerLeftRoom(Player otherPlayer)
        {
            if (!PhotonBattle.IsRealPlayer(otherPlayer))
            {
                return; // Ignore non players
            }
            var teamIndex = PhotonBattle.GetTeamIndex(PhotonBattle.GetPlayerPos(otherPlayer));
            if (teamIndex == _playerActorA.TeamIndex)
            {
                Hide();
            }
        }

        public IPlayerActor GetNearest()
        {
            Debug.Log($"GetNearest {_playerActorA} a={_distanceA} b={_distanceB}");
            if (_playerActorB == null || _distanceA < _distanceB)
            {
                return _playerActorA;
            }
            return _playerActorB;
        }

        public void Hide()
        {
            Debug.Log($"Hide {_playerActorA}");
            gameObject.SetActive(false);
        }
    }
}