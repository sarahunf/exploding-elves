using UnityEngine;
using UnityEditor;
using System.IO;
using System.Linq;

public class UnusedAssetFinder : EditorWindow
{
    [MenuItem("Tools/Platform Optimization/Find Unused Assets")]
    public static void FindUnusedAssets()
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
} 