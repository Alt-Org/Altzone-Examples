using System;
using Altzone.Scripts.Config;
using Altzone.Scripts.Model;
using UnityEditor;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Editor.Prg.GameDebug
{
    public static class DebugMenu
    {
        [MenuItem("Window/ALT-Zone/Game Debug/Show Player Data", false, 1)]
        private static void ShowLocalPlayerData()
        {
            Debug.Log("*");
            var playerData = RuntimeGameConfig.GetPlayerDataCacheInEditor();
            Debug.Log(playerData.ToString());
        }

        [MenuItem("Window/ALT-Zone/Game Debug/Create Dummy Player Data", false, 2)]
        private static void CreateDummyPlayerData()
        {
            var playerData = RuntimeGameConfig.GetPlayerDataCacheInEditor();
            playerData.BatchSave(() =>
            {
                playerData.PlayerName = $"Player{1000 * (1 + DateTime.Now.Second % 10) + DateTime.Now.Millisecond:00}";
                playerData.Language = Application.systemLanguage;
                playerData.CharacterModelId = Random.Range((int)Defence.Desensitisation, (int)Defence.Confluence + 1);
            });
            Debug.Log(playerData.ToString());
        }

        [MenuItem("Window/ALT-Zone/Game Debug/Delete Player Data", false, 3)]
        private static void DeleteLocalPlayerData()
        {
            Debug.Log("*");
            var playerData = RuntimeGameConfig.GetPlayerDataCacheInEditor();
            playerData.BatchSave(() =>
            {
                // Actually can not delete!
                playerData.PlayerName = string.Empty;
                playerData.Language = SystemLanguage.Unknown;
                playerData.CharacterModelId = -1;
            });
            Debug.Log(playerData.ToString());
        }

        [MenuItem("Window/ALT-Zone/Game Debug/Danger Zone/Delete All Local Data", false, 1)]
        private static void DeleteLocalAllData()
        {
            Debug.Log("*");
            Debug.Log(RichText.Brown("PlayerPrefs.DeleteAll"));
            PlayerPrefs.DeleteAll();
            PlayerPrefs.Save();
        }
    }
}