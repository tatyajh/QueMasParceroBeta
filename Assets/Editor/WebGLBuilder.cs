using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

//Genera el build WebGL desde linea de comandos:
//Unity.exe -batchmode -quit -executeMethod WebGLBuilder.Build
public class WebGLBuilder
{
    public static void Build()
    {
        PlayerSettings.productName = "¡Qué Más Parcero!";

        //Gzip con fallback: la combinacion mas compatible con itch.io
        PlayerSettings.WebGL.compressionFormat = WebGLCompressionFormat.Gzip;
        PlayerSettings.WebGL.decompressionFallback = true;
        PlayerSettings.runInBackground = true;

        BuildPlayerOptions options = new BuildPlayerOptions
        {
            scenes = new[] { "Assets/Scenes/GameScene.unity" },
            locationPathName = "Builds/WebGL",
            target = BuildTarget.WebGL,
            options = BuildOptions.None
        };

        BuildReport report = BuildPipeline.BuildPlayer(options);
        Debug.Log("Resultado del build: " + report.summary.result +
                  " | Tamaño: " + (report.summary.totalSize / (1024 * 1024)) + " MB");

        if (report.summary.result != BuildResult.Succeeded)
        {
            EditorApplication.Exit(1);
        }
    }
}
