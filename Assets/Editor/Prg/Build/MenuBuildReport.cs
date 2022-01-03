using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;

namespace Editor.Prg.Build
{
    public class MenuBuildReport : MonoBehaviour
    {
        private static readonly string[] ExcludedFolders =
        {
            "Assets/Photon",
            "Assets/Photon Unity Networking",
            "Assets/Editor",
            "Assets/TextMesh Pro",
        };

        [MenuItem("Window/ALT-Zone/Build/Create Build Script", false, 1)]
        private static void CreateBuildScript()
        {
            TeamCity.CreateBuildScript();
        }

        [MenuItem("Window/ALT-Zone/Build/Test Android Build Config", false, 2)]
        private static void CheckAndroidBuild()
        {
            TeamCity.CheckAndroidBuild();
        }

        [MenuItem("Window/ALT-Zone/Build/Check Build Report", false, 9)]
        private static void WindowReport()
        {
            var logWriter = new LogWriter();

            var buildTargetName = TeamCity.CommandLine.BuildTargetNameFrom(EditorUserBuildSettings.activeBuildTarget);
            logWriter.Log($"Build target is {buildTargetName}");
            var buildReport = $"m_Build_{buildTargetName}.log";
            if (!File.Exists(buildReport))
            {
                Debug.LogWarning($"Build report {buildReport} not found");
                return;
            }

            var allFiles = ParseBuildReport(buildReport, out var totalSize);
            logWriter.Log($"Build contains {allFiles.Count} total files, their size is {totalSize:### ### ##0.0} kb");
            var usedAssets = new HashSet<string>();
            int[] fileCount = { 0, 0, 0, 0 };
            double[] fileSize = { 0, 0, 0, 0 };
            double[] filePercent = { 0, 0, 0, 0 };
            foreach (var assetLine in allFiles)
            {
                if (assetLine.IsAsset)
                {
                    usedAssets.Add(assetLine.FilePath);
                    fileCount[0] += 1;
                    fileSize[0] += assetLine.FileSizeKb;
                    filePercent[0] += assetLine.Percentage;
                }
                else if (assetLine.IsPackage)
                {
                    fileCount[1] += 1;
                    fileSize[1] += assetLine.FileSizeKb;
                    filePercent[1] += assetLine.Percentage;
                }
                else if (assetLine.IsResource)
                {
                    fileCount[2] += 1;
                    fileSize[2] += assetLine.FileSizeKb;
                    filePercent[2] += assetLine.Percentage;
                }
                else if (assetLine.IsBuiltIn)
                {
                    fileCount[3] += 1;
                    fileSize[3] += assetLine.FileSizeKb;
                    filePercent[3] += assetLine.Percentage;
                }
                else
                {
                    Debug.LogError("Unknown asset line: " + assetLine.FilePath);
                    return;
                }
            }
            logWriter.Log($"Build contains {fileCount[0]} ASSET files, their size is {fileSize[0]:### ### ##0.0} kb ({filePercent[0]:0.0}%)");
            logWriter.Log($"Build contains {fileCount[1]} PACKAGE files, their size is {fileSize[1]:### ### ##0.0} kb ({filePercent[1]:0.0}%)");
            logWriter.Log($"Build contains {fileCount[2]} RESOURCE files, their size is {fileSize[2]:### ### ##0.0} kb ({filePercent[2]:0.0}%)");
            if (fileCount[3] > 0)
            {
                logWriter.Log($"Build contains {fileCount[3]} Built-in files, their size is {fileSize[3]:### ### ##0.0} kb ({filePercent[3]:0.0}%)");
            }
            var unusedAssets = CheckUnusedAssets(usedAssets, logWriter);
            logWriter.Log($"Project contains {unusedAssets.Count} unused assets for {buildTargetName} build");
            if (_excludedAssetCount > 0)
            {
                logWriter.Log($"Excluded {_excludedAssetCount} files or folders");
            }
            double unusedAssetSizeTotal = unusedAssets.Select(x => x.FileSizeKb).Sum();
            logWriter.Log($"Unused assets total size is {unusedAssetSizeTotal:### ### ##0.0} kb");
            foreach (var unusedAsset in unusedAssets.OrderBy(x => x.FileSizeKb).Reverse())
            {
                logWriter.Log($"UNUSED\t{unusedAsset.FilePath}\t{unusedAsset.FileSizeKb:### ### ##0.0} kb");
            }
            var reportName = $"{Path.GetFileNameWithoutExtension(buildReport)}_report.log";
            logWriter.Save(reportName);
            Debug.Log($"Report save in {reportName}");
        }

        private static int _excludedAssetCount;

        private static List<AssetLine> CheckUnusedAssets(HashSet<string> usedAssets, LogWriter logWriter)
        {
            void HandleSubFolder(string parent, List<Regex> excluded, ref List<AssetLine> result)
            {
                if (ExcludedFolders.Contains(parent))
                {
                    _excludedAssetCount += 1;
                    return;
                }
                string[] guids = AssetDatabase.FindAssets(null, new[] { parent });
                foreach (var guid in guids)
                {
                    var assetPath = AssetDatabase.GUIDToAssetPath(guid);
                    var isExclude = false;
                    foreach (var regex in excluded)
                    {
                        if (regex.IsMatch(assetPath))
                        {
                            logWriter.Log($"skip {assetPath} ({regex})");
                            isExclude = true;
                            break;
                        }
                    }
                    if (isExclude)
                    {
                        _excludedAssetCount += 1;
                    }
                    else
                    {
                        var isUsed = usedAssets.Contains(assetPath);
                        if (!isUsed)
                        {
                            if (Directory.Exists(assetPath))
                            {
                                continue; // Ignore folders
                            }
                            var assetLine = new AssetLine(assetPath, isFile: true);
                            if (!result.Contains(assetLine))
                            {
                                result.Add(assetLine);
                            }
                        }
                    }
                }
            }

            var excludedList = new List<Regex>();
            foreach (var excludedFolder in ExcludedFolders)
            {
                excludedList.Add(new Regex(excludedFolder));
            }
            _excludedAssetCount = 0;
            var resultList = new List<AssetLine>();
            var folders = AssetDatabase.GetSubFolders("Assets");
            foreach (var folder in folders)
            {
                HandleSubFolder(folder, excludedList, ref resultList);
            }
            return resultList;
        }

        private static List<AssetLine> ParseBuildReport(string buildReport, out double totalSize)
        {
            const string markerLine = "-------------------------------------------------------------------------------";
            const string assetsLine = "Used Assets and files from the Resources folder, sorted by uncompressed size:";
            var result = new List<AssetLine>();
            var processing = false;
            totalSize = 0;
            foreach (var line in File.ReadAllLines(buildReport))
            {
                if (processing)
                {
                    if (line == markerLine)
                    {
                        break;
                    }
                    var assetLine = new AssetLine(line);
                    totalSize += assetLine.FileSizeKb;
                    result.Add(assetLine);
                }
                if (line == assetsLine)
                {
                    processing = true;
                }
            }
            return result;
        }

        private class AssetLine
        {
            private static readonly CultureInfo Culture = CultureInfo.GetCultureInfo("en-US");
            private static readonly char[] Separators1 = { '%' };
            private static readonly char[] Separators2 = { ' ' };

            private readonly string _line;
            public readonly double FileSizeKb;
            public readonly double Percentage;
            public readonly string FilePath;

            public bool IsAsset => FilePath.StartsWith("Assets/");
            public bool IsPackage => FilePath.StartsWith("Packages/");
            public bool IsResource => FilePath.StartsWith("Resources/");
            public bool IsBuiltIn => FilePath.StartsWith("Built-in ");

            public AssetLine(string line, bool isFile = false)
            {
                _line = line ?? string.Empty;
                if (isFile)
                {
                    FilePath = _line;
                    FileSizeKb = (int)(new FileInfo(FilePath).Length / 1024);
                    return;
                }
                var tokens = _line.Split(Separators1);
                if (tokens.Length != 2)
                {
                    FilePath = _line;
                    return;
                }
                FilePath = tokens[1].Trim();
                tokens = tokens[0].Split(Separators2, StringSplitOptions.RemoveEmptyEntries);
                if (tokens.Length != 3)
                {
                    return;
                }
                FileSizeKb = double.Parse(tokens[0], Culture);
                Percentage = double.Parse(tokens[2], Culture);
            }

            private bool Equals(AssetLine other)
            {
                return _line == other._line;
            }

            public override bool Equals(object obj)
            {
                if (ReferenceEquals(null, obj))
                {
                    return false;
                }
                if (ReferenceEquals(this, obj))
                {
                    return true;
                }
                if (obj.GetType() != this.GetType())
                {
                    return false;
                }
                return Equals((AssetLine)obj);
            }

            public override int GetHashCode()
            {
                return _line.GetHashCode();
            }
        }

        private class LogWriter
        {
            private readonly StringBuilder _builder = new StringBuilder();

            public void Log(string line)
            {
                Debug.Log(line);
                _builder.Append(line).AppendLine();
            }

            public void Save(string fileName)
            {
                File.WriteAllText(fileName, _builder.ToString());
            }
        }
    }
}