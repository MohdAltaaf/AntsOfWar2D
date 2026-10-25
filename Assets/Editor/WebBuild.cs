#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEngine;

public static class WebBuild
{
    public static void Build()
    {
        const string outputPath = "Builds/WebGL";
        Directory.CreateDirectory(outputPath);

        EditorUserBuildSettings.SwitchActiveBuildTarget(
            BuildTargetGroup.WebGL, BuildTarget.WebGL);
        PlayerSettings.WebGL.compressionFormat = WebGLCompressionFormat.Disabled;
        PlayerSettings.WebGL.decompressionFallback = true;
        PlayerSettings.WebGL.dataCaching = true;

        var report = BuildPipeline.BuildPlayer(new BuildPlayerOptions
        {
            scenes = new[]
            {
                "Assets/Scenes/MainMenu.unity",
                "Assets/Scenes/World.unity"
            },
            locationPathName = outputPath,
            target = BuildTarget.WebGL,
            options = BuildOptions.None
        });

        if (report.summary.result != UnityEditor.Build.Reporting.BuildResult.Succeeded)
        {
            Debug.LogError($"WebGL build failed: {report.summary.result}");
            EditorApplication.Exit(1);
            return;
        }

        Debug.Log($"WebGL build complete: {report.summary.totalSize} bytes at {outputPath}");
        EditorApplication.Exit(0);
    }
}
#endif
