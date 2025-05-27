using UnityEngine;
using UnityEditor;

public class TextureImportOptimizer : EditorWindow
{
    [MenuItem("Tools/Platform Optimization/Optimize Texture Import Settings")]
    public static void OptimizeTextures()
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
} 