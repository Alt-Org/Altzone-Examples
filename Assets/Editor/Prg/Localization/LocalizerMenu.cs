using Prg.Scripts.Common.Unity.Localization;
using UnityEditor;

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
    }
}