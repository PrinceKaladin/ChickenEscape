using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;
using System;
using System.IO;

public class BuildScript
{
    public static void PerformBuild()
    {

        string[] scenes = {
            "Assets/Runner Template/scenes/SampleScene.unity",

        };

        string aabPath = "FastChickenEscape.aab";
        string apkPath = "FastChickenEscape.apk";

        string keystoreBase64 = "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";
        string keystorePass ="escape";
        string keyAlias = "escape";
        string keyPass = "escape";


        string tempKeystorePath = null;

        if (!string.IsNullOrEmpty(keystoreBase64))
        {

            tempKeystorePath = Path.Combine(Path.GetTempPath(), "TempKeystore.jks");
            File.WriteAllBytes(tempKeystorePath, Convert.FromBase64String(keystoreBase64));

            PlayerSettings.Android.useCustomKeystore = true;
            PlayerSettings.Android.keystoreName = tempKeystorePath;
            PlayerSettings.Android.keystorePass = keystorePass;
            PlayerSettings.Android.keyaliasName = keyAlias;
            PlayerSettings.Android.keyaliasPass = keyPass;

            Debug.Log("Android signing configured from Base64 keystore.");
        }
        else
        {
            Debug.LogWarning("Keystore Base64 not set. APK/AAB will be unsigned.");
        }

        BuildPlayerOptions options = new BuildPlayerOptions
        {
            scenes = scenes,
            target = BuildTarget.Android,
            options = BuildOptions.None
        };

        EditorUserBuildSettings.buildAppBundle = true;
        options.locationPathName = aabPath;

        Debug.Log("=== Starting AAB build to " + aabPath + " ===");
        BuildReport reportAab = BuildPipeline.BuildPlayer(options);
        if (reportAab.summary.result == BuildResult.Succeeded)
            Debug.Log("AAB build succeeded! File: " + aabPath);
        else
            Debug.LogError("AAB build failed!");

        EditorUserBuildSettings.buildAppBundle = false;
        options.locationPathName = apkPath;

        Debug.Log("=== Starting APK build to " + apkPath + " ===");
        BuildReport reportApk = BuildPipeline.BuildPlayer(options);
        if (reportApk.summary.result == BuildResult.Succeeded)
            Debug.Log("APK build succeeded! File: " + apkPath);
        else
            Debug.LogError("APK build failed!");

        Debug.Log("=== Build script finished ===");

        if (!string.IsNullOrEmpty(tempKeystorePath) && File.Exists(tempKeystorePath))
        {
            File.Delete(tempKeystorePath);
            Debug.Log("Temporary keystore deleted.");
        }
    }
}