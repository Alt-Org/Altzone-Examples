using Examples.Config.Scripts;
using Examples.Game.Scripts.Battle.Room;
using Examples.Game.Scripts.Battle.Scene;
using Photon.Pun;
using Prg.Scripts.Common.PubSub;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Examples.Game.Scripts.Battle.Player
{
    public interface IPlayerActor
    {
        int PlayerPos { get; }
        bool IsLocal { get; }
        int TeamMatePos { get; }
        int TeamIndex { get; }
        bool IsLocalTeam { get; }
        int OppositeTeam { get; }
        IPlayerActor TeamMate { get; }
        void setNormalMode();
        void setFrozenMode();
        void setGhostedMode();
        void showShield();
        void hideShield();
        void headCollision();
        float CurrentSpeed { get; }
    }

    /// <summary>
    /// Player base class for common player data.
    /// </summary>
    public class PlayerActor : MonoBehaviour, IPlayerActor
    {
        public static List<IPlayerActor> allPlayerActors;

        private const int playModeNormal = 0;
        private const int playModeFrozen = 1;
        private const int playModeGhosted = 2;

        [Header("Settings"), SerializeField] private GameObject[] shields;
        [SerializeField] private GameObject realPlayer;
        [SerializeField] private GameObject frozenPlayer;
        [SerializeField] private GameObject ghostPlayer;
        [SerializeField] private GameObject localHighlight;

        [Header("Live Data"), SerializeField] private int playerPos;
        [SerializeField] private int teamIndex;
        [SerializeField] private bool isLocal;
        //[SerializeField] private int currentMode;
        [SerializeField] private Collider2D[] colliders;

        int IPlayerActor.PlayerPos => playerPos;
        bool IPlayerActor.IsLocal => isLocal;
        int IPlayerActor.TeamMatePos => getTeamMatePos(playerPos);
        int IPlayerActor.TeamIndex => teamIndex;
        bool IPlayerActor.IsLocalTeam => allPlayerActors.Any(x => x.TeamIndex == teamIndex && x.IsLocal);
        int IPlayerActor.OppositeTeam => teamIndex == 0 ? 1 : 0;
        IPlayerActor IPlayerActor.TeamMate => allPlayerActors.FirstOrDefault(x => x.TeamIndex == teamIndex && x.PlayerPos != playerPos);
        float IPlayerActor.CurrentSpeed => _Speed;

        private float _Speed;
        private int myShieldIndex;
        private bool isShieldVisible;
        private IRestrictedPlayer restrictedPlayer;
        private PhotonView _photonView;

        public int sortKey => 10 * teamIndex + playerPos;

        private void Awake()
        {
            _photonView = PhotonView.Get(this);
            isLocal = _photonView.IsMine;
            var player = _photonView.Owner;
            PhotonBattle.getPlayerProperties(player, out playerPos, out teamIndex);
            var model = PhotonBattle.getPlayerCharacterModel(player);
            _Speed = model.Speed;
            Debug.Log($"Awake {player.NickName} pos={playerPos} team={teamIndex}");
            myShieldIndex = teamIndex;
            shields[((IPlayerActor)this).OppositeTeam].SetActive(false);
            colliders = GetComponentsInChildren<Collider2D>(includeInactive: false);

            // Re-parent and set name
            var sceneConfig = SceneConfig.Get();
            transform.parent = sceneConfig.actorParent.transform;
            name = $"{(player.IsLocal ? "L" : "R")}{playerPos}:{teamIndex}:{player.NickName}";

            setupPlayer(player);
        }

        private void setupPlayer(Photon.Realtime.Player player)
        {
            Debug.Log($"setupPlayer pos={playerPos} team={teamIndex} {player.GetDebugLabel()}");
            // Setup input system to move player around - PlayerMovement is required on both ends for RPC!
            var playerMovement = gameObject.AddComponent<PlayerMovement>();
            restrictedPlayer = playerMovement;
            if (player.IsLocal)
            {
                setupLocalPlayer(playerMovement);
            }
            else
            {
                setupRemotePlayer();
            }
        }

        private void setupLocalPlayer(IMovablePlayer movablePlayer)
        {
            var sceneConfig = SceneConfig.Get();
            var playArea = sceneConfig.getPlayArea(playerPos);
            restrictedPlayer.setPlayArea(playArea);

            var playerInput = gameObject.AddComponent<PlayerInput>();
            playerInput.Camera = sceneConfig._camera;
            playerInput.PlayerMovement = movablePlayer;
            if (!Application.isMobilePlatform)
            {
                var keyboardInput = gameObject.AddComponent<PlayerInputKeyboard>();
                keyboardInput.PlayerMovement = movablePlayer;
            }

            localHighlight.SetActive(true);
        }

        private void setupRemotePlayer()
        {
            localHighlight.SetActive(false);
        }

        private void OnEnable()
        {
            Debug.Log($"OnEnable pos={playerPos} team={teamIndex}");
        }

        private void OnDisable()
        {
            Debug.Log($"OnDisable pos={playerPos} team={teamIndex}");
        }

        void IPlayerActor.setNormalMode()
        {
            //if (currentMode != playModeNormal)
            if (PhotonNetwork.IsMasterClient)
            {
                _photonView.RPC(nameof(setPlayerPlayModeRpc), RpcTarget.All, playModeNormal);
            }
        }

        void IPlayerActor.setFrozenMode()
        {
            //if (currentMode != playModeFrozen)
            if (PhotonNetwork.IsMasterClient)
            {
                _photonView.RPC(nameof(setPlayerPlayModeRpc), RpcTarget.All, playModeFrozen);
            }
        }

        void IPlayerActor.setGhostedMode()
        {
            //if (currentMode != playModeGhosted)
            if (PhotonNetwork.IsMasterClient)
            {
                _photonView.RPC(nameof(setPlayerPlayModeRpc), RpcTarget.All, playModeGhosted);
            }
        }

        void IPlayerActor.headCollision()
        {
            Debug.Log($"headCollision pos={playerPos} team={teamIndex}");
            ((IPlayerActor)this).setGhostedMode();
            var oppositeTeam = ((IPlayerActor)this).OppositeTeam;
            ScoreManager.addHeadScore(oppositeTeam);
        }

        void IPlayerActor.showShield()
        {
            Debug.Log($"showShield pos={playerPos} team={teamIndex}");
            isShieldVisible = true;
            shields[myShieldIndex].SetActive(isShieldVisible);
        }

        void IPlayerActor.hideShield()
        {
            Debug.Log($"hideShield pos={playerPos} team={teamIndex}");
            isShieldVisible = false;
            shields[myShieldIndex].SetActive(isShieldVisible);
        }

        private void _setNormalMode()
        {
            Debug.Log($"setNormalMode pos={playerPos} team={teamIndex}");
            realPlayer.SetActive(true);
            frozenPlayer.SetActive(false);
            ghostPlayer.SetActive(false);
            shields[myShieldIndex].SetActive(isShieldVisible);
            restrictedPlayer.canMove = true;
        }

        private void _setFrozenMode()
        {
            Debug.Log($"setFrozenMode pos={playerPos} team={teamIndex}");
            realPlayer.SetActive(false);
            frozenPlayer.SetActive(true);
            ghostPlayer.SetActive(false);
            shields[myShieldIndex].SetActive(false);
            restrictedPlayer.canMove = false;
        }

        private void _setGhostedMode()
        {
            Debug.Log($"setGhostedMode pos={playerPos} team={teamIndex}");
            realPlayer.SetActive(false);
            frozenPlayer.SetActive(false);
            ghostPlayer.SetActive(true);
            shields[myShieldIndex].SetActive(false);
            restrictedPlayer.canMove = true;
        }

        [PunRPC]
        private void setPlayerPlayModeRpc(int playMode)
        {
            switch (playMode)
            {
                case playModeNormal:
                    _setNormalMode();
                    return;
                case playModeFrozen:
                    _setFrozenMode();
                    return;
                case playModeGhosted:
                    _setGhostedMode();
                    return;
                default:
                    throw new UnityException($"unknown play mode: {playMode}");
            }
        }

        private static int getTeamMatePos(int playerPos)
        {
            switch (playerPos)
            {
                case 0:
                    return 2;
                case 1:
                    return 3;
                case 2:
                    return 0;
                case 3:
                    return 1;
                default:
                    throw new UnityException($"invalid player pos: {playerPos}");
            }
        }
    }
}