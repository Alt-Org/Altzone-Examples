using Photon.Pun;
using Prg.Scripts.Common.PubSub;
using System;
using UnityEngine;

namespace Examples.Game.Scripts.Battle.Player
{
    /// <summary>
    /// Interface to restrict player movement to given area.
    /// </summary>
    public interface IRestrictedPlayer
    {
        void setPlayArea(Rect area);
    }

    /// <summary>
    /// Simple player movement using external controller for movement and synchronized across network using <c>RPC</c>.
    /// </summary>
    /// <remarks>
    /// Player movement is restricted to given area.
    /// </remarks>
    [RequireComponent(typeof(PhotonView))]
    public class PlayerMovement : MonoBehaviour, IMovablePlayer, IRestrictedPlayer
    {
        [Header("Live Data"), SerializeField] protected PhotonView _photonView;
        [SerializeField] protected Transform _transform;

        [SerializeField] private bool canMove;
        [SerializeField] private bool isMoving;
        [SerializeField] private Vector3 validTarget;
        [SerializeField] private Vector3 inputTarget;
        [SerializeField] private Rect playArea;
        [SerializeField] private float speed;

        private void Awake()
        {
            _photonView = PhotonView.Get(this);
            _transform = GetComponent<Transform>();
            canMove = true;
        }

        private void Update()
        {
            if (!isMoving)
            {
                return;
            }
            if (!canMove)
            {
                return;
            }
            var nextPosition = Vector3.MoveTowards(_transform.position, validTarget, speed * Time.deltaTime);
            isMoving = nextPosition != validTarget;
            _transform.position = nextPosition;
        }

        void IMovablePlayer.moveTo(Vector3 position)
        {
            if (!canMove)
            {
                return;
            }
            if (position.Equals(inputTarget))
            {
                return;
            }
            inputTarget = position;
            position.x = Mathf.Clamp(inputTarget.x, playArea.xMin, playArea.xMax);
            position.y = Mathf.Clamp(inputTarget.y, playArea.yMin, playArea.yMax);
            // Send position to all players
            _photonView.RPC(nameof(MoveTowardsRpc), RpcTarget.All, position);
        }

        void IRestrictedPlayer.setPlayArea(Rect area)
        {
            playArea = area;
        }

        [PunRPC]
        private void MoveTowardsRpc(Vector3 targetPosition)
        {
            isMoving = true;
            validTarget = targetPosition;
        }
    }
}