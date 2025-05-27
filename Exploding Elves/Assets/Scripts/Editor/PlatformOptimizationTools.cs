using UnityEngine;
using UnityEditor;
using System.IO;
using System.Linq;
using UnityEngine.Rendering;

public class PlatformOptimizationTools : EditorWindow
{
    [MenuItem("Tools/Platform Optimization Tools")]
    public static void ShowWindow()
    {
        GetWindow<PlatformOptimizationTools>("Platform Optimization Tools");
    }

    void OnGUI()
    {
        if (GUILayout.Button("Optimize Texture Import Settings")) OptimizeTextures();
        if (GUILayout.Button("Optimize Audio Import Settings")) OptimizeAudio();
        if (GUILayout.Button("Set Build Settings")) SetBuildSettings();
        if (GUILayout.Button("Set Player Settings")) SetPlayerSettings();
        if (GUILayout.Button("Find Unused Assets")) FindUnusedAssets();
        GUILayout.Space(10);
        if (GUILayout.Button("Run All Optimizations")) RunAll();
    }

    // 1. Texture Import Settings
    static void OptimizeTextures()
    {
        string[] textureGuids = AssetDatabase.FindAssets("t:Texture", new[] { "Assets" });
        foreach (string guid in textureGuids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;
            if (importer != null)
            {
                // Android
                TextureImporterPlatformSettings androidSettings = importer.GetPlatformTextureSettings("Android");
                androidSettings.overridden = true;
                androidSettings.format = TextureImporterFormat.ETC2_RGBA8;
                importer.SetPlatformTextureSettings(androidSettings);
                // iOS
                TextureImporterPlatformSettings iosSettings = importer.GetPlatformTextureSettings("iPhone");
                iosSettings.overridden = true;
                iosSettings.format = TextureImporterFormat.ASTC_6x6;
                importer.SetPlatformTextureSettings(iosSettings);
                EditorUtility.SetDirty(importer);
                importer.SaveAndReimport();
            }
        }
        Debug.Log("Texture import settings optimized for Android and iOS.");
    }

    // 2. Audio Import Settings
    static void OptimizeAudio()
    {
        string[] audioGuids = AssetDatabase.FindAssets("t:AudioClip", new[] { "Assets" });
        foreach (string guid in audioGuids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            AudioImporter importer = AssetImporter.GetAtPath(path) as AudioImporter;
            if (importer != null)
            {
                AudioImporterSampleSettings settings = importer.defaultSampleSettings;
                settings.compressionFormat = AudioCompressionFormat.Vorbis;
                settings.loadType = AudioClipLoadType.CompressedInMemory;
                settings.quality = 0.5f;
                importer.defaultSampleSettings = settings;
                EditorUtility.SetDirty(importer);
                importer.SaveAndReimport();
            }
        }
        Debug.Log("Audio import settings optimized.");
    }

    // 3. Build Settings
    static void SetBuildSettings()
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

    // 4. Player Settings
    static void SetPlayerSettings()
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

    // 5. Remove Unused Assets (Detection Only)
    static void FindUnusedAssets()
    {
        string[] allAssets = AssetDatabase.GetAllAssetPaths().Where(p => p.StartsWith("Assets/")).ToArray();
        string[] usedAssets = AssetDatabase.GetDependencies("Assets", true);
        var unused = allAssets.Except(usedAssets).Where(p => !p.EndsWith(".cs") && !Directory.Exists(p)).ToList();
        if (unused.Count == 0)
        {
            Debug.Log("No unused assets found.");
        }
        else
        {
            Debug.LogWarning($"Unused assets detected:\n{string.Join("\n", unused)}");
        }
    }

    static void RunAll()
    {
        OptimizeTextures();
        OptimizeAudio();
        SetBuildSettings();
        SetPlayerSettings();
        FindUnusedAssets();
        Debug.Log("All optimizations complete.");
    }
} 