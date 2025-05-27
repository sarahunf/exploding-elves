using UnityEngine;
using UnityEditor;
using UnityEngine.Rendering;

public class PlayerSettingsOptimizer : EditorWindow
{
    [MenuItem("Tools/Platform Optimization/Set Player Settings")]
    public static void SetPlayerSettings()
    {
        // Android
        PlayerSettings.defaultInterfaceOrientation = UIOrientation.LandscapeLeft;
        PlayerSettings.Android.targetSdkVersion = AndroidSdkVersions.AndroidApiLevel30;
        PlayerSettings.SetGraphicsAPIs(BuildTarget.Android, new[] { GraphicsDeviceType.Vulkan, GraphicsDeviceType.OpenGLES3 });
        // iOS
        PlayerSettings.SetGraphicsAPIs(BuildTarget.iOS, new[] { GraphicsDeviceType.Metal });
        PlayerSettings.iOS.targetOSVersionString = "12.0";
        Debug.Log("Player settings set for Android and iOS.");
    }
} 