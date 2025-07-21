using UnityEditor;
using UnityEngine;
using System.IO;
using System.Collections.Generic;

public class ForceIncludeBuilder
{
    [MenuItem("Tools/Build SmartTV Bundle (Fixed No Warnings)")]
    public static void BuildBundle()
    {
        string outputDir = "G:\\Steam\\steamapps\\common\\Subnautica\\BepInEx\\plugins\\SmartTV";
        if (!Directory.Exists(outputDir))
        { 
            Directory.CreateDirectory(outputDir);
        }

        string tvRootPath = "Assets/Prefabs/TVRoot.prefab";
        string theatherTvRootPath = "Assets/Prefabs/TheaterTVRoot.prefab";

        var assetPaths = new List<string>();
        assetPaths.Add(tvRootPath);
        assetPaths.Add("Assets/Resources/TVSprite.png");

        if (AssetDatabase.CopyAsset(tvRootPath, theatherTvRootPath))
        {
            assetPaths.Add(theatherTvRootPath);
        }

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
