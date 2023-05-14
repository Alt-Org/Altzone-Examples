using System;
using System.Collections.Generic;
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
            Debug.Log("BuildPlayer start");
            var batchBuildOptions = GetBatchBuildOptions();
            var buildReport = BuildPLayer(batchBuildOptions);
            SaveBuildReport(buildReport);
            Debug.Log($"BuildPlayer exit {buildReport.summary.result}");
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
                locationPathName = Path.Combine(options.ProjectPath, options.OutputDir),
                options = options.BuildOptions,
                scenes = scenes,
                target = options.BuildTarget,
                targetGroup = options.BuildTargetGroup,
            };
            if (Directory.Exists(buildPlayerOptions.locationPathName))
            {
                Directory.Delete(buildPlayerOptions.locationPathName, recursive: true);
            }
            var buildReport = BuildPipeline.BuildPlayer(buildPlayerOptions);
            return buildReport;
        }

        private static void SaveBuildReport(BuildReport buildReport)
        {
            if (buildReport.summary.result != BuildResult.Succeeded)
            {
                return;
            }
            throw new NotImplementedException();
        }

        private static BatchBuildOptions GetBatchBuildOptions()
        {
            return new BatchBuildOptions(Environment.GetCommandLineArgs());
        }

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
            public readonly string OutputDir;
            public readonly string Keystore;

            // Just for information, if needed.
            public readonly bool IsDevelopmentBuild;

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
                    }
                }

                BuildOptions = BuildOptions.DetailedBuildReport | (IsDevelopmentBuild ? BuildOptions.Development : 0);
                OutputDir = Path.Combine(ProjectPath, $"build{BuildPipeline.GetBuildTargetName(BuildTarget)}");
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
        }

#if false
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
                "./",
                "-buildTarget",
                "Win64",
                "-logFile",
                @"m_Build_Win64.log",
                "-envFile",
                @"m_Build_Win64.env"
            };
            var test = new BatchBuildOptions(args);
            Debug.Log(test.ToString());
        }
#endif
    }
}
