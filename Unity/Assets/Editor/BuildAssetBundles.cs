using UnityEditor;
using UnityEngine;
using System.IO;
using System.Collections.Generic;

public class ForceIncludeBuilder
{
    [MenuItem("Tools/Build SmartTV Bundle (Fixed No Warnings)")]
    public static void BuildBundle()
    {
        string outputDir = "Assets/generatedAssetBundles";
        if (!Directory.Exists(outputDir))
        { 
            Directory.CreateDirectory(outputDir);
        }

        var assetPaths = new List<string>();
        assetPaths.Add("Assets/Prefabs/TVRoot.prefab");
        assetPaths.Add("Assets/Resources/pause.png");
        assetPaths.Add("Assets/Resources/play.png");
        assetPaths.Add("Assets/Resources/TVSprite.png");

        var build = new AssetBundleBuild {
            assetBundleName = "60insmarttv",
            assetNames = assetPaths.ToArray()
        };

        BuildPipeline.BuildAssetBundles(
            outputDir,
            new[] { build },
            BuildAssetBundleOptions.ForceRebuildAssetBundle,
            BuildTarget.StandaloneWindows
        );

        Debug.Log($"✅ Built SmartTV bundle. {assetPaths.Count} assets packed.");
    }
}
