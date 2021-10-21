using Examples.Config.Scripts;
using Examples.Game.Scripts.Battle.Room;
using Examples.Game.Scripts.Battle.Scene;
using Photon.Pun;
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
        private const int playModeNormal = 0;
        private const int playModeFrozen = 1;
        private const int playModeGhosted = 2;

        [Header("Settings"), SerializeField] private GameObject[] shields;
        [SerializeField] private GameObject realPlayer;
        [SerializeField] private GameObject frozenPlayer;
        [SerializeField] private GameObject ghostPlayer;
        [SerializeField] private GameObject localHighlight;

        [Header("Live Data"), SerializeField] private PlayerActivator activator;
        [SerializeField] private bool _isLocalTeam;
        [SerializeField] private PlayerActor _teamMate;

        int IPlayerActor.PlayerPos => activator.playerPos;
        bool IPlayerActor.IsLocal => activator.isLocal;
        int IPlayerActor.TeamMatePos => activator.teamMatePos;
        int IPlayerActor.TeamIndex => activator.teamIndex;
        int IPlayerActor.OppositeTeam => activator.oppositeTeamIndex;
        bool IPlayerActor.IsLocalTeam => _isLocalTeam;
        IPlayerActor IPlayerActor.TeamMate => _teamMate;
        float IPlayerActor.CurrentSpeed => _Speed;

        private float _Speed;
        private int myShieldIndex;
        private bool isShieldVisible;
        private IRestrictedPlayer restrictedPlayer;
        private PhotonView _photonView;

        private void Awake()
        {
            activator = GetComponent<PlayerActivator>();
            _photonView = PhotonView.Get(this);
            var player = _photonView.Owner;
            var model = PhotonBattle.getPlayerCharacterModel(player);
            _Speed = model.Speed;

            // Re-parent and set name
            var sceneConfig = SceneConfig.Get();
            transform.parent = sceneConfig.actorParent.transform;
            name = $"{(player.IsLocal ? "L" : "R")}{activator.playerPos}:{activator.teamIndex}:{player.NickName}";

            myShieldIndex = activator.teamIndex;
            shields[activator.oppositeTeamIndex].SetActive(false);

            setupPlayer(player);
        }

        public void LateAwake() // Called after all players have been "activated"
        {
            Debug.Log($"LateAwake name={name}");
            // Check out team status
            _isLocalTeam = activator.checkLocalTeam();
            _teamMate = activator.getTeamMate() as PlayerActor; // This is for Editor debugging, should be IPlayerActor OFC
            // Start shields as they require that all players are ready.
            var playerShield = GetComponentsInChildren<PlayerShield>(includeInactive: true);
            if (playerShield.Length == 1)
            {
                playerShield[0].enabled = true;
            }
        }

        private void setupPlayer(Photon.Realtime.Player player)
        {
            Debug.Log($"setupPlayer {player.GetDebugLabel()}");
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
            var playArea = sceneConfig.getPlayArea(activator.playerPos);
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
            Debug.Log($"OnEnable name={name}");
        }

        private void OnDisable()
        {
            Debug.Log($"OnDisable name={name}");
        }

        void IPlayerActor.setNormalMode()
        {
            if (PhotonNetwork.IsMasterClient)
            {
                _photonView.RPC(nameof(setPlayerPlayModeRpc), RpcTarget.All, playModeNormal);
            }
        }

        void IPlayerActor.setFrozenMode()
        {
            if (PhotonNetwork.IsMasterClient)
            {
                _photonView.RPC(nameof(setPlayerPlayModeRpc), RpcTarget.All, playModeFrozen);
            }
        }

        void IPlayerActor.setGhostedMode()
        {
            if (PhotonNetwork.IsMasterClient)
            {
                _photonView.RPC(nameof(setPlayerPlayModeRpc), RpcTarget.All, playModeGhosted);
            }
        }

        void IPlayerActor.headCollision()
        {
            Debug.Log($"headCollision name={name}");
            ((IPlayerActor)this).setGhostedMode();
            var oppositeTeam = ((IPlayerActor)this).OppositeTeam;
            ScoreManager.addHeadScore(oppositeTeam);
        }

        void IPlayerActor.showShield()
        {
            Debug.Log($"showShield name={name}");
            isShieldVisible = true;
            shields[myShieldIndex].SetActive(isShieldVisible);
        }

        void IPlayerActor.hideShield()
        {
            Debug.Log($"hideShield name={name}");
            isShieldVisible = false;
            shields[myShieldIndex].SetActive(isShieldVisible);
        }

        private void _setNormalMode()
        {
            Debug.Log($"setNormalMode name={name}");
            realPlayer.SetActive(true);
            frozenPlayer.SetActive(false);
            ghostPlayer.SetActive(false);
            shields[myShieldIndex].SetActive(isShieldVisible);
            restrictedPlayer.canMove = true;
        }

        private void _setFrozenMode()
        {
            Debug.Log($"setFrozenMode name={name}");
            realPlayer.SetActive(false);
            frozenPlayer.SetActive(true);
            ghostPlayer.SetActive(false);
            shields[myShieldIndex].SetActive(false);
            restrictedPlayer.canMove = false;
        }

        private void _setGhostedMode()
        {
            Debug.Log($"setGhostedMode name={name}");
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
    }
}