using UnityEditor;

namespace Prg.Editor.Build
{
    internal static class BatchBuild
    {
        internal static void BuildPlayer()
        {
            Debug.Log("BuildPlayer");
            EditorApplication.Exit(0);
        }
    }
}
