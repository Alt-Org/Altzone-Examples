using UnityEditor;

namespace Prg.Editor.Build
{
    internal static class MenuBuildReportAnalyzer
    {
        [MenuItem("Altzone/Show Last Build Report", false, 10)]
        private static void ShowLastBuildReport() => BuildReportAnalyzer.HtmlBuildReportFull();
    }
}
