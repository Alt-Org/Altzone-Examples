using System;
using System.Collections.Generic;
using System.Diagnostics;
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
    internal static class BuildReportLogFileParser
    {
        internal static void SaveBuildReport(BuildReport buildReport)
        {
            const string assetFolder = "Assets/BuildReports";

            Debug.Log("report start");
            var stopWatch = Stopwatch.StartNew();
            var d = buildReport.summary.buildEndedAt;
            var dateString = $"{d.Year}-{d.Month:00}-{d.Day:00}_{d.Minute:00}.{d.Second:00}";
            var assetReportName = $"{assetFolder}/Build_{dateString}.buildreport";

            // Create asset based 'Build Report'
            AssetDatabase.CreateAsset(buildReport, assetReportName);
            var assetReport = AssetDatabase.LoadAssetAtPath<BuildReport>(assetReportName);
            stopWatch.Stop();
            Debug.Log($"create report {assetReportName} time {stopWatch.Elapsed.TotalMinutes:0.0} m");
            Assert.IsNotNull(assetReport);
        }

        internal static BuildReportLog ParseBuildReport(DateTime buildEndedAt, string buildLogFilename)
        {
            const string buildReportLine = "Build Report";
            const string usedAssetsLine = "Used Assets and files from the Resources folder, sorted by uncompressed size:";
            const string noDataMarkerLine = "Information on used Assets is not available, since player data was not rebuilt.";
            const string endMarkerLine = "-------------------------------------------------------------------------------";
            // Example lines:
            //  1.4 mb	 0.2% Assets/Altzone/Graphics/Logo/ALT ZONE logo.png
            //  341.5 kb	 0.1% Assets/TextMesh Pro/Sprites/EmojiOne.png
            //  0.1 kb	 0.0% Assets/MenuUi/Scripts/Shop.cs

            var filename = Path.GetFileName(buildLogFilename);
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
                return new BuildReportLog(buildEndedAt, -1, null);
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
                    return new BuildReportLog(buildEndedAt, -1, null);
                }
            }
            if (currentLine == lastLine)
            {
                Debug.LogWarning($"Report file {buildLogFilename} does not have valid 'Build Report'");
                return new BuildReportLog(buildEndedAt, -1, null);
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
                if (Filter.IsLogLineExcluded(line))
                {
                    continue;
                }
                var assetLine = new BuildReportLogLine(line);
                if (Filter.IsLogLineExcluded(line, assetLine.FileSizeKb))
                {
                    continue;
                }
                totalSize += assetLine.FileSizeKb;
                result.Add(assetLine);
            }
            return new BuildReportLog(buildEndedAt, totalSize, result);
        }
    }

    internal class BuildReportLog
    {
        public readonly DateTime BuildEndedAt;
        public readonly double TotalFileSizeKb;
        public readonly List<BuildReportLogLine> BuildReportLogLines;

        public BuildReportLog(DateTime buildEndedAt, double totalFileSizeKb, List<BuildReportLogLine> buildReportLogLines)
        {
            BuildEndedAt = buildEndedAt;
            TotalFileSizeKb = totalFileSizeKb;
            BuildReportLogLines = buildReportLogLines ?? new List<BuildReportLogLine>();
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

    internal static class Filter
    {
        internal static bool IsLogLineExcluded(string line)
        {
            return line.Contains("% Packages/") ||
                   line.Contains("% Resources/") ||
                   line.EndsWith(".cs");
        }
        internal static bool IsLogLineExcluded(string line, double sizeKb)
        {
            return sizeKb < 1.0 || line.Contains("% Assets/TextMesh Pro/");
        }

        internal static bool IsAssetPathExcluded(string path, ulong packedSize)
        {
            // Note that
            // - scenes are not included in Build Report as other assets
            // - inputactions can not be detected for now and we ignore them silently
            if (packedSize < 1024UL)
            {
                return true;
            }
            if (path.StartsWith("Assets/TextMesh Pro/"))
            {
                return packedSize < 1024UL * 1024UL;
            }
            return path.StartsWith("Packages/") ||
                   path.StartsWith("Assets/BuildReport") ||
                   path.StartsWith("Assets/Photon/") ||
                   path.StartsWith("Assets/Plugins/") ||
                   path.StartsWith("Assets/Tests/") ||
                   path.Contains("/Editor/") ||
                   path.Contains("/Test/") ||
                   path.EndsWith(".asmdef") ||
                   path.EndsWith(".asmref") ||
                   path.EndsWith(".controller") ||
                   path.EndsWith(".cs") ||
                   path.EndsWith(".inputactions") ||
                   path.EndsWith(".preset") ||
                   path.EndsWith(".unity");
        }
    }
#if true
    internal static class Menu
    {
        [MenuItem("Altzone/Batch Build Report Test (log)", false, 19)]
        private static void BatchBuildReportLogFileTest()
        {
            const string outputFileName = "m_Build_Win64.log.tsv";

            Debug.Log($"* write {outputFileName}");
            var culture = CultureInfo.InvariantCulture;
            var logReport = BuildReportLogFileParser.ParseBuildReport(DateTime.Now, "m_Build_Win64.log");
            Assert.IsNotNull(logReport);
            var builder = new StringBuilder()
                .Append("Name\tSize Kb\t%")
                .AppendLine()
                .Append($"Total Size\t{logReport.TotalFileSizeKb.ToString("0.0", culture)}")
                .AppendLine();
            foreach (var line in logReport.BuildReportLogLines)
            {
                builder.Append(line.FilePath).Append('\t')
                    .Append(line.FileSizeKb.ToString("0.0", culture)).Append('\t')
                    .Append(line.Percentage > 0 ? line.Percentage.ToString("0.0", culture) : "0")
                    .AppendLine();
            }
            // Remove last CR-LF.
            builder.Length -= 2;
            File.WriteAllText(outputFileName, builder.ToString());
        }

        [MenuItem("Altzone/Batch Build Report Test (asset)", false, 19)]
        private static void BatchBuildReportAssetFileTest()
        {
            const string outputFileName = "m_Build_Win64.report.tsv";

            Debug.Log($"* write {outputFileName}");
            var culture = CultureInfo.InvariantCulture;
            var buildReport = BuildReportAnalyzer.GetOrCreateLastBuildReport();

            var packedAssets = LoadAssetsFromBuildReport(buildReport);
            var reportedSize = packedAssets.Sum(x => (long)x.packedSize);
            var builder = new StringBuilder()
                .Append("Name\tSize Kb\t%")
                .AppendLine()
                .Append($"Report Size\t{(reportedSize / 1024D).ToString("0.0", culture)}")
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
            File.WriteAllText(outputFileName, builder.ToString());
        }

        private static List<PackedAssetInfo> LoadAssetsFromBuildReport(BuildReport buildReport)
        {
            List<PackedAssetInfo> allBuildAssets = new List<PackedAssetInfo>();
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
                    if (Filter.IsAssetPathExcluded(assetInfo.sourceAssetPath, assetInfo.packedSize))
                    {
                        continue;
                    }
                    allBuildAssets.Add(assetInfo);
                }
            }
            return allBuildAssets;
        }
    }
#endif
}
