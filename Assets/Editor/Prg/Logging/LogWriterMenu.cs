using System;
using System.IO;
using System.Linq;
using Prg.Scripts.Common.Util;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace Editor.Prg.Logging
{
    public class LogWriterMenu : MonoBehaviour
    {
        [MenuItem("Window/ALT-Zone/Debug Logging/Add 'FORCE_LOG' define")]
        private static void AddDefine()
        {
            Debug.Log("*");
            var knownTargets = new[] { BuildTarget.Android, BuildTarget.StandaloneWindows64, BuildTarget.WebGL };
            var count = AddScriptingDefineSymbolToAllBuildTargetGroups("FORCE_LOG", knownTargets);
            if (count == 0)
            {
                Debug.Log("FORCE_LOG seems to be defined already on all of our build targets");
            }
            else if (count == knownTargets.Length)
            {
                Debug.Log("FORCE_LOG define has been added to all of our build targets");
            }
            else
            {
                Debug.Log("FORCE_LOG define has been added to some of our build targets");
            }
        }

        [MenuItem("Window/ALT-Zone/Debug Logging/Remove 'FORCE_LOG' define")]
        private static void RemoveDefine()
        {
            Debug.Log("*");
            var knownTargets = new[] { BuildTarget.Android, BuildTarget.StandaloneWindows64, BuildTarget.WebGL };
            var count = RemoveScriptingDefineSymbolToAllBuildTargetGroups("FORCE_LOG", knownTargets);
            if (count == 0)
            {
                Debug.Log("FORCE_LOG define was not found on any of our build targets");
            }
            else if (count == knownTargets.Length)
            {
                Debug.Log("FORCE_LOG define has been removed from all of our build targets");
            }
            else
            {
                Debug.Log("FORCE_LOG define has been removed from some of our build targets");
            }
        }

        [MenuItem("Window/ALT-Zone/Debug Logging/Highlight Settings")]
        private static void HighlightSettings()
        {
            LoggerConfig loggerConfig = (LoggerConfig)Resources.Load(nameof(LoggerConfig), typeof(LoggerConfig));
            Selection.objects = new UnityEngine.Object[] { loggerConfig };
            EditorGUIUtility.PingObject(loggerConfig);
        }

        [MenuItem("Window/ALT-Zone/Debug Logging/Open file in text editor")]
        private static void Load()
        {
            Debug.Log("*");
            var path = GetLogFilePath();
            if (File.Exists(path))
            {
                InternalEditorUtility.OpenFileAtLineExternal(path, 1);
            }
        }

        [MenuItem("Window/ALT-Zone/Debug Logging/Show log file location")]
        private static void Show()
        {
            Debug.Log("*");
            GetLogFilePath();
        }

        private static string GetLogFilePath()
        {
            var path = Path.Combine(Application.persistentDataPath, LogWriter.GetLogName());
            if (Application.platform.ToString().ToLower().Contains("windows"))
            {
                path = path.Replace(Path.AltDirectorySeparatorChar.ToString(), Path.DirectorySeparatorChar.ToString());
            }
            Debug.Log($"Editor log {(File.Exists(path) ? "is in" : RichText.Brown("NOT found"))}: {path}");
            return path;
        }

        /// <summary>
        /// Adds a given scripting define symbol to all build target groups
        /// You can see all scripting define symbols ( not the internal ones, only the one for this project), in the PlayerSettings inspector
        /// </summary>
        /// <param name="defineSymbol">Define symbol.</param>
        /// <param name="targets">Build targets to modify</param>
        private static int AddScriptingDefineSymbolToAllBuildTargetGroups(string defineSymbol, BuildTarget[] targets)
        {
            var count = 0;
            foreach (var target in targets)
            {
                var group = BuildPipeline.GetBuildTargetGroup(target);
                if (group == BuildTargetGroup.Unknown)
                {
                    continue;
                }
                var defineSymbols = PlayerSettings.GetScriptingDefineSymbolsForGroup(group).Split(';').Select(d => d.Trim()).ToList();
                if (defineSymbols.Contains(defineSymbol))
                {
                    Debug.Log($"Found already {defineSymbol} on: {target} in group: {@group}");
                    continue;
                }
                defineSymbols.Add(defineSymbol);
                try
                {
                    PlayerSettings.SetScriptingDefineSymbolsForGroup(@group, string.Join(";", defineSymbols.ToArray()));
                    Debug.Log($"Add {defineSymbol} to: {target} in group: {@group}");
                    count += 1;
                }
                catch (Exception e)
                {
                    Debug.Log($"Could not set Photon {defineSymbol} define for build target: {target} group: {@group}: {e}");
                }
            }
            return count;
        }

        private static int RemoveScriptingDefineSymbolToAllBuildTargetGroups(string defineSymbol, BuildTarget[] targets)
        {
            var count = 0;
            foreach (var target in targets)
            {
                var group = BuildPipeline.GetBuildTargetGroup(target);
                if (group == BuildTargetGroup.Unknown)
                {
                    continue;
                }
                var defineSymbols = PlayerSettings.GetScriptingDefineSymbolsForGroup(group).Split(';').Select(d => d.Trim()).ToList();
                if (!defineSymbols.Contains(defineSymbol))
                {
                    Debug.Log($"Not Found {defineSymbol} on: {target} in group: {@group}");
                    continue;
                }
                defineSymbols.Remove(defineSymbol);
                try
                {
                    PlayerSettings.SetScriptingDefineSymbolsForGroup(@group, string.Join(";", defineSymbols.ToArray()));
                    Debug.Log($"Remove {defineSymbol} from: {target} in group: {@group}");
                    count += 1;
                }
                catch (Exception e)
                {
                    Debug.Log($"Could not remove Photon {defineSymbol} define for build target: {target} group: {@group}: {e}");
                }
            }
            return count;
        }
    }
}