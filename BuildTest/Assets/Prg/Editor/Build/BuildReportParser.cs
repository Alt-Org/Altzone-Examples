using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;
using UnityEngine.Assertions;

namespace Prg.Editor.Build
{
#if true
    internal static class Menu
    {
        [MenuItem("Altzone/Batch Build Report Test (log)", false, 19)]
        private static void BatchBuildReportLogFileTest()
        {
            Debug.Log("*");
            var logFile = File.Exists("m_Build_Win64_build.log")
                ? "m_Build_Win64_build.log"
                : "m_Build_Win64.log";
            BuildReportParser.SaveBuildReport(logFile, "m_Build_Win64.log.test.tsv");
        }

        [MenuItem("Altzone/Batch Build Report Test (asset)", false, 19)]
        private static void BatchBuildReportAssetFileTest()
        {
            Debug.Log("*");
            var buildReport = BuildReportParser.GetOrCreateLastBuildReport();
            BuildReportParser.SaveBuildReport(buildReport, "m_Build_Win64.report.test.tsv");
        }
    }
#endif

    internal static class BuildReportParser
    {
        internal const string LastBuildReportPath = "Library/LastBuild.buildreport";
        private const string BuildReportDir = "Assets/BuildReports";

        internal static void SaveBuildReport(BuildReport buildReport, string outputFilename)
        {
            var culture = CultureInfo.InvariantCulture;

            var assetReport = ParseBuildReport(buildReport);
            var packedAssets = assetReport.BuildReportAssetLines;
            var builder = new StringBuilder()
                .Append($"Name\tSize Kb\t%\tFiles\t{packedAssets.Count}\tReported Size\t{(assetReport.TotalFileSizeKb).ToString("0.0", culture)}")
                .AppendLine();
            packedAssets = packedAssets.OrderBy(x => x.packedSize).Reverse().ToList();
            foreach (var asset in packedAssets)
            {
                builder.Append(asset.sourceAssetPath).Append('\t')
                    .Append((asset.packedSize / 1024D).ToString("0.0", culture))
                    .AppendLine();
            }
            // Remove last CR-LF.
            builder.Length -= 2;
            File.WriteAllText(outputFilename, builder.ToString());
            Debug.Log($"* write {packedAssets.Count} lines {outputFilename}");
        }

        internal static void SaveBuildReport(string buildLogFilename, string outputFilename)
        {
            var culture = CultureInfo.InvariantCulture;
            var logReport = ParseBuildReport(buildLogFilename);
            Assert.IsNotNull(logReport);
            var logLines = logReport.BuildReportLogLines;
            var builder = new StringBuilder()
                .Append($"Name\tSize Kb\t%\tFiles\t{logLines.Count}\tReported Size\t{logReport.TotalFileSizeKb.ToString("0.0", culture)}")
                .AppendLine();
            foreach (var line in logLines)
            {
                builder.Append(line.FilePath).Append('\t')
                    .Append(line.FileSizeKb.ToString("0.0", culture)).Append('\t')
                    .Append(line.Percentage > 0 ? line.Percentage.ToString("\t0.0", culture) : null)
                    .AppendLine();
            }
            // Remove last CR-LF.
            builder.Length -= 2;
            File.WriteAllText(outputFilename, builder.ToString());
            Debug.Log($"* write {logLines.Count} lines {buildLogFilename} -> {outputFilename}");
        }

        /// <summary>
        /// Gets last UNITY Build Report from file.
        /// </summary>
        /// <remarks>
        /// This code is based on UNITY Build Report Inspector<br />
        /// https://docs.unity3d.com/Packages/com.unity.build-report-inspector@0.1/manual/index.html<br />
        /// https://github.com/Unity-Technologies/BuildReportInspector/blob/master/com.unity.build-report-inspector/Editor/BuildReportInspector.cs
        /// </remarks>
        /// <returns>the last <c>BuildReport</c> instance or <c>null</c> if one is not found</returns>
        public static BuildReport GetOrCreateLastBuildReport()
        {
            if (!File.Exists(LastBuildReportPath))
            {
                Debug.Log($"Last Build Report NOT FOUND: {LastBuildReportPath}");
                return null;
            }
            if (!Directory.Exists(BuildReportDir))
            {
                Directory.CreateDirectory(BuildReportDir);
            }

            var date = File.GetLastWriteTime(LastBuildReportPath);
            var name = $"Build_{date:yyyy-dd-MM_HH.mm.ss}";
            var assetPath = $"{BuildReportDir}/{name}.buildreport";

            // Load last Build Report.
            var buildReport = AssetDatabase.LoadAssetAtPath<BuildReport>(assetPath);
            if (buildReport != null && buildReport.name == name)
            {
                return buildReport;
            }
            // Create new last Build Report.
            File.Copy("Library/LastBuild.buildreport", assetPath, true);
            AssetDatabase.ImportAsset(assetPath);
            buildReport = AssetDatabase.LoadAssetAtPath<BuildReport>(assetPath);
            buildReport.name = name;
            AssetDatabase.SaveAssets();
            return buildReport;
        }

        private static BuildReportAssets ParseBuildReport(BuildReport buildReport)
        {
            List<PackedAssetInfo> packedAssets = new List<PackedAssetInfo>();
            foreach (var packedAsset in buildReport.packedAssets)
            {
                var contents = packedAsset.contents;
                foreach (var assetInfo in contents)
                {
                    if (assetInfo.type == typeof(MonoBehaviour))
                    {
                        continue;
                    }
                    var sourceAssetGuid = assetInfo.sourceAssetGUID.ToString();
                    if (sourceAssetGuid == "00000000000000000000000000000000" || sourceAssetGuid == "0000000000000000f000000000000000")
                    {
                        continue;
                    }
                    packedAssets.Add(assetInfo);
                }
            }
            var reportedSize = packedAssets.Sum(x => (long)x.packedSize);
            return new BuildReportAssets(reportedSize / 1024D, packedAssets);
        }

        private static BuildReportLog ParseBuildReport(string buildLogFilename)
        {
            const string buildReportLine = "Build Report";
            const string usedAssetsLine = "Used Assets and files from the Resources folder, sorted by uncompressed size:";
            const string noDataMarkerLine = "Information on used Assets is not available, since player data was not rebuilt.";
            const string endMarkerLine = "-------------------------------------------------------------------------------";
            // Example lines:
            //  1.4 mb	 0.2% Assets/Altzone/Graphics/Logo/ALT ZONE logo.png
            //  341.5 kb	 0.1% Assets/TextMesh Pro/Sprites/EmojiOne.png
            //  0.1 kb	 0.0% Assets/MenuUi/Scripts/Shop.cs

            var lines = File.ReadAllLines(buildLogFilename);
            var lastLine = lines.Length - 1;
            var currentLine = 0;
            // Find Build Report line.
            for (; currentLine < lastLine; ++currentLine)
            {
                var line = lines[currentLine];
                if (line == buildReportLine)
                {
                    break;
                }
            }
            if (currentLine == lastLine)
            {
                Debug.LogWarning($"Report file {buildLogFilename} does not have a 'Build Report'");
                return new BuildReportLog(-1, null);
            }
            // Find Used Assets line.
            currentLine += 1;
            for (; currentLine < lastLine; ++currentLine)
            {
                var line = lines[currentLine];
                if (line == usedAssetsLine)
                {
                    break;
                }
                if (line == noDataMarkerLine)
                {
                    Debug.LogWarning($"Report file {buildLogFilename} did not have data" +
                                     $" for 'Used Assets' because <b>player data was not rebuilt</b> on last build!");
                    return new BuildReportLog(-1, null);
                }
            }
            if (currentLine == lastLine)
            {
                Debug.LogWarning($"Report file {buildLogFilename} does not have valid 'Build Report'");
                return new BuildReportLog(-1, null);
            }
            // Read all Used Assets lines.
            var totalSize = 0D;
            var result = new List<BuildReportLogLine>();
            currentLine += 1;
            for (; currentLine < lastLine; ++currentLine)
            {
                var line = lines[currentLine];
                if (line == endMarkerLine)
                {
                    break;
                }
                var assetLine = new BuildReportLogLine(line);
                totalSize += assetLine.FileSizeKb;
                result.Add(assetLine);
            }
            return new BuildReportLog(totalSize, result);
        }
    }

    internal class BuildReportLog
    {
        public readonly double TotalFileSizeKb;
        public readonly List<BuildReportLogLine> BuildReportLogLines;

        public BuildReportLog(double totalFileSizeKb, List<BuildReportLogLine> buildReportLogLines)
        {
            TotalFileSizeKb = totalFileSizeKb;
            BuildReportLogLines = buildReportLogLines ?? new List<BuildReportLogLine>();
        }
    }

    internal class BuildReportAssets
    {
        public readonly double TotalFileSizeKb;
        public readonly List<PackedAssetInfo> BuildReportAssetLines;

        public BuildReportAssets(double totalFileSizeKb, List<PackedAssetInfo> buildReportAssetLines)
        {
            TotalFileSizeKb = totalFileSizeKb;
            BuildReportAssetLines = buildReportAssetLines;
        }
    }

    internal class BuildReportLogLine
    {
        private static readonly CultureInfo Culture = CultureInfo.GetCultureInfo("en-US");
        private static readonly char[] Separators1 = { '%' };
        private static readonly char[] Separators2 = { ' ', '\t' };

        public readonly double FileSizeKb;
        public readonly double Percentage;
        public readonly string FilePath;

        public BuildReportLogLine(string logLine)
        {
            //  1.4 mb	 1.6% Assets/Sounds/Altzone_battle_version_2.mp3
            //  712.4 kb	 0.8% Assets/Sounds/10-minutes-of-silence.mp3

            Assert.IsTrue(!string.IsNullOrWhiteSpace(logLine));
            // Guaranteed split of the line onto two parts - filename can have spaces in it!
            var tokens = logLine.Split(Separators1);
            Assert.AreEqual(2, tokens.Length);
            // Last part is the file name.
            FilePath = tokens[1].Trim();
            // First part contains three tokens.
            tokens = tokens[0].Split(Separators2, StringSplitOptions.RemoveEmptyEntries);
            Assert.AreEqual(3, tokens.Length);
            FileSizeKb = double.Parse(tokens[0], Culture);
            if (tokens[1] == "mb")
            {
                FileSizeKb *= 1024.0;
            }
            else
            {
                Assert.AreEqual("kb", tokens[1]);
            }
            Percentage = double.Parse(tokens[2], Culture);
        }
    }
}
