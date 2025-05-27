using UnityEngine;
using UnityEditor;

public class AudioImportOptimizer : EditorWindow
{
    [MenuItem("Tools/Platform Optimization/Optimize Audio Import Settings")]
    public static void OptimizeAudio()
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
} 