#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEditor.Build.Reporting;

namespace SoulForge.Editor
{
    public static class SoulForgeBuildMenu
    {
        [MenuItem("Tools/SoulForge/Build Host")]
        public static void BuildHost()
        {
            Build(new[]
            {
                "Assets/Game/Scenes/Booster.unity",
                "Assets/Game/Scenes/Hub.unity",
                "Assets/Game/Scenes/Floor_01.unity",
                "Assets/Game/Scenes/Floor_02.unity",
                "Assets/Game/Scenes/Floor_03.unity"
            }, "Builds/Host/SoulForgeHost.exe");
        }

        [MenuItem("Tools/SoulForge/Build Viewer")]
        public static void BuildViewer()
        {
            Build(new[]
            {
                "Assets/Game/Scenes/Viewer_Client.unity",
                "Assets/Game/Scenes/Viewer_Live.unity"
            }, "Builds/Viewer/SoulForgeViewer.exe");
        }

        private static void Build(string[] scenes, string outputPath)
        {
            string fullDirectory = Path.GetDirectoryName(outputPath);
            if (!string.IsNullOrWhiteSpace(fullDirectory) && !Directory.Exists(fullDirectory))
            {
                Directory.CreateDirectory(fullDirectory);
            }

            BuildPlayerOptions options = new()
            {
                scenes = scenes,
                locationPathName = outputPath,
                target = BuildTarget.StandaloneWindows64,
                options = BuildOptions.None
            };

            BuildReport report = BuildPipeline.BuildPlayer(options);
            if (report.summary.result == BuildResult.Succeeded)
            {
                UnityEngine.Debug.Log($"Build succeeded: {outputPath}");
            }
            else
            {
                UnityEngine.Debug.LogError($"Build failed: {outputPath}");
            }
        }
    }
}
#endif
