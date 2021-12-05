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
    }
}