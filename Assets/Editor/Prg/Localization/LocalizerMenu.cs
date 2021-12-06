using Prg.Scripts.Common.Unity.Localization;
using UnityEditor;

namespace Editor.Prg.Localization
{
    public static class LocalizerMenu
    {
        [MenuItem("Window/ALT-Zone/Localization/Load Translations (bin)", false, 1)]
        private static void LoadTranslations()
        {
            Debug.Log("*");
            Localizer.LoadTranslations();
        }

        [MenuItem("Window/ALT-Zone/Localization/Save Translations (tsv->bin)", false, 2)]
        private static void SaveTranslations()
        {
            Debug.Log("*");
            Localizer.SaveTranslations();
        }

        [MenuItem("Window/ALT-Zone/Localization/Show Translations (bin)", false, 3)]
        private static void ShowTranslations()
        {
            Debug.Log("*");
            Localizer.ShowTranslations();
        }
    }
}