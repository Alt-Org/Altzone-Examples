using System;
using System.Collections.Generic;
using System.Linq;
using Altzone.Scripts.Config;
using Prg.Scripts.Common.Unity.Localization;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace Editor.Prg.Localization
{
    /// <summary>
    /// Localization process in Editor menu commands.
    /// </summary>
    public static class LocalizerMenu
    {
        public const string MenuRoot = "Window/ALT-Zone/Localization/";

        [MenuItem(MenuRoot + "Load Translations (bin)", false, 1)]
        private static void LoadTranslations()
        {
            Debug.Log("*");
            Localizer.LoadTranslations();
        }

        [MenuItem(MenuRoot + "Save Translations (tsv->bin)", false, 2)]
        private static void SaveTranslations()
        {
            Debug.Log("*");
            Localizer.LocalizerHelper.SaveTranslations();
        }

        [MenuItem(MenuRoot + "Show Translations (bin)", false, 3)]
        private static void ShowTranslations()
        {
            Debug.Log("*");
            Localizer.LocalizerHelper.ShowTranslations();
        }

        [MenuItem(MenuRoot + "Check Used Translations In Assets", false, 4)]
        private static void CheckUsedTranslationsInAssets()
        {
            Debug.Log("*");
            var playerData = RuntimeGameConfig.GetPlayerDataCacheInEditor();
            var language = playerData.HasLanguageCode ? playerData.Language : Localizer.DefaultLanguage;
            if (!Localizer.HasLanguage(language))
            {
                Localizer.LoadTranslations();
            }
            Localizer.SetLanguage(language);
            DoSmartTextAndTextAssetCheck();
            Localizer.LocalizerHelper.SaveIfDirty();
        }

        private static void DoSmartTextAndTextAssetCheck()
        {
            var allAssets = AssetDatabase.GetAllAssetPaths().Where(path => path.StartsWith("Assets/"));
            var gameObjects = allAssets.Select(a =>
                AssetDatabase.LoadAssetAtPath(a, typeof(GameObject)) as GameObject).Where(a => a != null).ToList();
            Debug.Log($"Total assets {gameObjects.Count} to check");
            var result = new List<SmartTextContext>();
            foreach (var gameObject in gameObjects)
            {
                var components = gameObject.GetComponentsInChildren<Text>(true);
                foreach (var component in components)
                {
                    CheckGameObject(gameObject, component.gameObject, ref result);
                }
            }
            Debug.Log($"Checked assets {result.Count}");
            result.Sort((a, b) => String.Compare(a.sortKey, b.sortKey, StringComparison.Ordinal));
            foreach (var smartTextContext in result)
            {
                smartTextContext.ShowMissing();
            }
        }

        private static void CheckGameObject(GameObject parent, GameObject child, ref List<SmartTextContext> context)
        {
            var smartText = child.GetComponent<SmartText>();
            if (smartText == null)
            {
                context.Add(new SmartTextContext(parent, "2 Text is not localized: ", child));
                return;
            }
            context.Add(new SmartTextContext(parent, "1 SmartText: ", child));
            SmartText.TrackWords(smartText);
        }

        private class SmartTextContext
        {
            private readonly GameObject _parent;
            private readonly string _message;
            private readonly string _childPath;

            public readonly string sortKey;

            public SmartTextContext(GameObject parent, string message, GameObject child)
            {
                _parent = parent;
                _message = message;
                _childPath = child.GetFullPath();
                sortKey = message + _childPath;
            }

            public void ShowMissing()
            {
                Debug.LogWarning($"{_message}{_childPath}", _parent);
            }
        }
    }
}