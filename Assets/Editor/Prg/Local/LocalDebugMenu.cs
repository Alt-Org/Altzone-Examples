using System;
using Altzone.Scripts.Config;
using Altzone.Scripts.Model;
using NUnit.Framework.Internal.Filters;
using UnityEditor;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Editor.Prg.Local
{
    public static class LocalDebugMenu
    {
        [MenuItem("Window/ALT-Zone/Local/Delete Local All Data")]
        private static void DeleteLocalAllData()
        {
            Debug.Log("*");
        }

        [MenuItem("Window/ALT-Zone/Local/Show Player Data")]
        private static void ShowLocalPlayerData()
        {
            Debug.Log("*");
            var playerData = RuntimeGameConfig.GetPlayerDataCacheInEditor();
            Debug.Log(playerData.ToString());
        }

        [MenuItem("Window/ALT-Zone/Local/Delete Player Data")]
        private static void DeleteLocalPlayerData()
        {
            Debug.Log("*");
            var playerData = RuntimeGameConfig.GetPlayerDataCacheInEditor();
            playerData.BatchSave(() =>
            {
                // Actually can not delete!
                playerData.PlayerName = string.Empty;
                playerData.CharacterModelId = -1;
            });
            Debug.Log(playerData.ToString());
        }

        [MenuItem("Window/ALT-Zone/Local/Create Dummy Player Data")]
        private static void CreateDummyPlayerData()
        {
            var playerData = RuntimeGameConfig.GetPlayerDataCacheInEditor();
            playerData.BatchSave(() =>
            {
                playerData.PlayerName = $"Player{1000 * (1 + DateTime.Now.Second % 10) + DateTime.Now.Millisecond:00}";
                playerData.CharacterModelId = Random.Range((int)Defence.Desensitisation, (int)Defence.Confluence + 1);
            });
            Debug.Log(playerData.ToString());
        }
    }
}