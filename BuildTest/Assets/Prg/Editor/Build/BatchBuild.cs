using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace Prg.Editor.Build
{
    /// <summary>
    /// Utility to build project with given setting from command line.<br />
    /// https://docs.unity3d.com/Manual/EditorCommandLineArguments.html<br />
    /// Following arguments are mandatory:<br />
    /// -projectPath<br />
    /// -buildTarget<br />
    /// -logFile<br />
    /// -envFile
    /// </summary>
    internal static class BatchBuild
    {
        internal static void BuildPlayer()
        {
            Debug.Log("build start");
            var options = new BatchBuildOptions(Environment.GetCommandLineArgs());
            Debug.Log($"batchBuildOptions {options}");
            if (options.IsTestRun)
            {
                Debug.Log("build exit 0");
                EditorApplication.Exit(0);
                return;
            }
            var stopWatch = Stopwatch.StartNew();
            var buildReport = BuildPLayer(options);
            if (buildReport.summary.result == BuildResult.Succeeded)
            {
                SaveBuildReport(buildReport, options.LogFile);
            }
            stopWatch.Stop();
            Debug.Log($"build exit result {buildReport.summary.result} time {stopWatch.Elapsed.TotalMinutes:0.0} m");
            EditorApplication.Exit(buildReport.summary.result == BuildResult.Succeeded ? 0 : 1);
        }

        private static BuildReport BuildPLayer(BatchBuildOptions options)
        {
            var scenes = EditorBuildSettings.scenes
                .Where(x => x.enabled)
                .Select(x => x.path)
                .ToArray();
            var buildPlayerOptions = new BuildPlayerOptions
            {
                locationPathName = options.OutputPathName,
                options = options.BuildOptions,
                scenes = scenes,
                target = options.BuildTarget,
                targetGroup = options.BuildTargetGroup,
            };
            if (Directory.Exists(options.OutputFolder))
            {
                Directory.Delete(options.OutputFolder, recursive: true);
            }
            Directory.CreateDirectory(options.OutputFolder);
            var buildReport = BuildPipeline.BuildPlayer(buildPlayerOptions);
            return buildReport;
        }

        private static void SaveBuildReport(BuildReport buildReport, string logFile)
        {
            Debug.Log("report start");
        }

        #region BatchBuildOptions

        private class BatchBuildOptions
        {
            // Paths and file names.
            public readonly string ProjectPath;
            public readonly string LogFile;
            public readonly string EnvFile;

            // Actual build settings etc.
            public readonly BuildTarget BuildTarget;
            public readonly BuildTargetGroup BuildTargetGroup;
            public readonly BuildOptions BuildOptions;
            public readonly string OutputPathName;
            public readonly string Keystore;

            // Just for information, if needed.
            public readonly string OutputFolder;
            public readonly bool IsDevelopmentBuild;

            public readonly bool IsTestRun;

            public BatchBuildOptions(string[] args)
            {
                // Parse command line arguments
                // -projectPath - project folder name (for UNITY)
                // -buildTarget - build target name (for UNITY)
                // -logFile - log file name (for UNITY)
                // -envFile - settings file name (for BatchBuild to read actual build options etc)
                {
                    var buildTargetName = string.Empty;
                    for (var i = 0; i < args.Length; ++i)
                    {
                        var arg = args[i];
                        switch (arg)
                        {
                            case "-projectPath":
                                i += 1;
                                ProjectPath = args[i];
                                if (!Directory.Exists(ProjectPath))
                                {
                                    throw new ArgumentException($"Directory -projectPath ${ProjectPath} is not found");
                                }
                                break;
                            case "-buildTarget":
                                i += 1;
                                buildTargetName = args[i];
                                break;
                            case "-logFile":
                                i += 1;
                                LogFile = args[i];
                                break;
                            case "-envFile":
                                i += 1;
                                EnvFile = args[i];
                                if (!File.Exists(EnvFile))
                                {
                                    throw new ArgumentException($"File -envFile '{EnvFile}' is not found");
                                }
                                break;
                        }
                    }
                    if (string.IsNullOrWhiteSpace(ProjectPath))
                    {
                        throw new ArgumentException($"-projectPath is mandatory for batch build");
                    }
                    if (KnownBuildTargets.TryGetValue(buildTargetName, out var buildTarget))
                    {
                        BuildTarget = buildTarget.Item1;
                        BuildTargetGroup = buildTarget.Item2;
                        switch (BuildTarget)
                        {
                            // Primary.
                            case BuildTarget.Android:
                                break;
                            // Secondary.
                            case BuildTarget.WebGL:
                                break;
                            // For testing only.
                            case BuildTarget.StandaloneWindows64:
                                break;
                            default:
                                throw new UnityException($"Build target '{BuildTarget}' is not supported");
                        }
                    }
                    else
                    {
                        throw new ArgumentException($"-buildTarget '{buildTargetName}' is invalid or unsupported");
                    }
                    if (string.IsNullOrWhiteSpace(LogFile))
                    {
                        throw new ArgumentException($"-logFile is mandatory for batch build");
                    }
                    if (string.IsNullOrWhiteSpace(EnvFile))
                    {
                        throw new ArgumentException($"-envFile is mandatory for batch build");
                    }
                }

                // Parse settings file.
                var lines = File.ReadAllLines(EnvFile);
                // Find Build Report line.
                foreach (var line in lines)
                {
                    if (line.StartsWith("#") || string.IsNullOrEmpty(line))
                    {
                        continue;
                    }
                    var tokens = line.Split('=', StringSplitOptions.RemoveEmptyEntries);
                    if (tokens.Length == 1 && line.Contains('='))
                    {
                        // Skip empty values!
                        continue;
                    }
                    var key = tokens[0].Trim();
                    var value = tokens[1].Trim();
                    switch (key)
                    {
                        case "IsDevelopmentBuild":
                            IsDevelopmentBuild = bool.Parse(value);
                            break;
                        case "Keystore":
                            // This requires actual keystore file name and two passwords!
                            Keystore = value;
                            break;
                        case "IsTestRun":
                            IsTestRun = bool.Parse(value);
                            break;
                    }
                }

                BuildOptions = BuildOptions.StrictMode | BuildOptions.DetailedBuildReport;
                if (IsDevelopmentBuild)
                {
                    BuildOptions |= BuildOptions.Development;
                }
                OutputFolder = Path.Combine(ProjectPath, $"build{BuildPipeline.GetBuildTargetName(BuildTarget)}");
                if (BuildTarget == BuildTarget.WebGL)
                {
                    OutputPathName = OutputFolder;
                    return;
                }
                var appName = SanitizePath($"{Application.productName}_{Application.version}_{PlayerSettings.Android.bundleVersionCode}");
                var appExtension = BuildTarget == BuildTarget.Android ? "aab" : "exe";
                OutputPathName = Path.Combine(OutputFolder, $"{appName}.{appExtension}");
            }

            public override string ToString()
            {
                return $"{nameof(ProjectPath)}: {ProjectPath}, {nameof(LogFile)}: {LogFile}, {nameof(EnvFile)}: {EnvFile}" +
                       $", {nameof(BuildTarget)}: {BuildTarget}, {nameof(BuildTargetGroup)}: {BuildTargetGroup}" +
                       $", {nameof(BuildOptions)}: [{BuildOptions}], {nameof(Keystore)}: {Keystore}" +
                       $", {nameof(OutputFolder)}: {OutputFolder}, {nameof(OutputPathName)}: {OutputPathName}" +
                       $", {nameof(IsDevelopmentBuild)}: {IsDevelopmentBuild}, {nameof(IsTestRun)}: {IsTestRun}";
            }

            // Build target parameter mapping
            // See: https://docs.unity3d.com/Manual/CommandLineArguments.html
            // See: https://docs.unity3d.com/2019.4/Documentation/ScriptReference/BuildTarget.html
            // See: https://docs.unity3d.com/ScriptReference/BuildPipeline.GetBuildTargetName.html
            private static readonly Dictionary<string, Tuple<BuildTarget, BuildTargetGroup>> KnownBuildTargets = new()
            {
                {
                    /*" Win64" */ BuildPipeline.GetBuildTargetName(BuildTarget.StandaloneWindows64),
                    new Tuple<BuildTarget, BuildTargetGroup>(BuildTarget.StandaloneWindows64, BuildTargetGroup.Standalone)
                },
                {
                    /*" Android" */ BuildPipeline.GetBuildTargetName(BuildTarget.Android),
                    new Tuple<BuildTarget, BuildTargetGroup>(BuildTarget.Android, BuildTargetGroup.Android)
                },
                {
                    /*" WebGL" */ BuildPipeline.GetBuildTargetName(BuildTarget.WebGL),
                    new Tuple<BuildTarget, BuildTargetGroup>(BuildTarget.WebGL, BuildTargetGroup.WebGL)
                },
            };

            private static string SanitizePath(string path)
            {
                // https://www.mtu.edu/umc/services/websites/writing/characters-avoid/
                var illegalCharacters = new[]
                {
                    '#', '<', '$', '+',
                    '%', '>', '!', '`',
                    '&', '*', '\'', '|',
                    '{', '?', '"', '=',
                    '}', '/', ':',
                    '\\', ' ', '@',
                };
                for (var i = 0; i < path.Length; ++i)
                {
                    var c = path[i];
                    if (illegalCharacters.Contains(c))
                    {
                        path = path.Replace(c, '_');
                    }
                }
                return path;
            }
        }

#if true
        [MenuItem("Altzone/Batch Build Options Test", false, 19)]
        private static void BatchBuildOptionsTest()
        {
            var args = new string[]
            {
                @"C:\Program Files\Unity\Hub\Editor\2021.3.21f1\Editor\Unity.exe",
                "-executeMethod Prg.Editor.Build.BatchBuild.BuildPlayer",
                "-quit",
                "-batchmode",
                "-projectPath",
                ".",
                "-buildTarget",
                "Win64",
                "-logFile",
                "m_Build_Win64.log",
                "-envFile",
                "m_Build_Win64.env"
            };
            var options = new BatchBuildOptions(args);
            Debug.Log(options.ToString());
            Debug.Log(Path.Combine(options.ProjectPath, options.OutputPathName));
        }
#endif

        #endregion
    }
}
