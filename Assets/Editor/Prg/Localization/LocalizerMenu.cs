using Prg.Scripts.Common.Unity.Localization;
using UnityEditor;

namespace Editor.Prg.Localization
{
    public static class LocalizerMenu
    {
        [MenuItem("Window/ALT-Zone/Localization/Show Localization", false, 1)]
        private static void ShowLocalPlayerData()
        {
            Debug.Log("*");
            Localizer.LoadTranslations();
        }

        [MenuItem("Window/ALT-Zone/Localization/Save Localization", false, 2)]
        private static void SaveLocalPlayerData()
        {
            Debug.Log("*");
            Localizer.SaveTranslations();
        }
    }
}