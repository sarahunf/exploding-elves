using UnityEngine;
using UnityEditor;

public class BuildSettingsOptimizer : EditorWindow
{
    [MenuItem("Tools/Platform Optimization/Set Build Settings")]
    public static void SetBuildSettings()
    {
        // Android
        EditorUserBuildSettings.androidBuildSystem = AndroidBuildSystem.Gradle;
        EditorUserBuildSettings.buildAppBundle = true;
        EditorUserBuildSettings.selectedBuildTargetGroup = BuildTargetGroup.Android;
        PlayerSettings.SetScriptingBackend(BuildTargetGroup.Android, ScriptingImplementation.IL2CPP);
        PlayerSettings.stripEngineCode = true;
        PlayerSettings.Android.targetArchitectures = AndroidArchitecture.ARMv7 | AndroidArchitecture.ARM64;
        // iOS
        PlayerSettings.SetScriptingBackend(BuildTargetGroup.iOS, ScriptingImplementation.IL2CPP);
        PlayerSettings.iOS.appleEnableAutomaticSigning = true;
        PlayerSettings.iOS.sdkVersion = iOSSdkVersion.DeviceSDK;
        Debug.Log("Build settings set for Android and iOS.");
    }
} 