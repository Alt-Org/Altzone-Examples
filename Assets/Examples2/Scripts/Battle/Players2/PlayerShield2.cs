using Altzone.Scripts.Battle;
using Examples2.Scripts.Battle.interfaces;
using UnityEngine;

namespace Examples2.Scripts.Battle.Players2
{
    internal class PlayerShield2 : IPlayerShield
    {
        private readonly ShieldConfig _config;

        public PlayerShield2(ShieldConfig config)
        {
            _config = config;
        }

        void IPlayerShield.SetupShield(int playerPos)
        {
            if (playerPos > PhotonBattle.PlayerPosition2)
            {
                var shield = _config.Shields[0];
                var renderer = shield.GetComponent<SpriteRenderer>();
                renderer.flipY = false;
            }
        }

        void IPlayerShield.SetShieldState(int playMode, int rotationIndex)
        {
            Debug.Log($"SetShieldState mode {playMode} rotation {rotationIndex}");
        }
    }
}