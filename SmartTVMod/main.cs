using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;
using Nautilus.Assets;
using Nautilus.Assets.Gadgets;
using Nautilus.Assets.PrefabTemplates;
using Nautilus.Crafting;
using Nautilus.Extensions;
using Nautilus.Utility;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine.Video;
using static Nautilus.Assets.PrefabTemplates.FabricatorTemplate;
using static RadicalLibrary.Spline;

namespace SmartTV
{

    [BepInPlugin("com.yourname.smarttvmod", "Smart TV Mod", "1.0.0")]
    public class SmartTVMain : BaseUnityPlugin
    {

        public static SmartTVMain instance;
        public static PrefabInfo smartTVInfo { get; } = PrefabInfo.WithTechType("SmartTV", "Smart TV", "A 60-inch Smart TV.");
        public GameObject tvPrefab;
        public GameObject tvPrefabParent;

        public string[] videos;
        private int currentVideoIndex = 0;
        private string videosFolderPath;

        private void Awake()
        {
            instance = this;
            string modFolder = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
            string bundlePath = modFolder + "/60insmarttv";
            videosFolderPath = modFolder + "/Videos/"; 
            UpdateVideosFolder(null, null);

            AssetBundle bundle = AssetBundle.LoadFromFile(bundlePath);
            if (bundle == null)
            {
                Logger.LogError("AssetBundle not found at: " + modFolder);
                return;
            }

            tvPrefab = bundle.LoadAsset<GameObject>("TVRoot");
            if (tvPrefab == null)
            { 
                Logger.LogError("TV prefab not found in AssetBundle.");
                return;
            }

            Sprite icon = bundle.LoadAsset<Sprite>("TVSprite");
            if (icon != null)
            {
                smartTVInfo.WithIcon(icon);
            }
            else
            {
                Debug.LogError("smarttv icon is null");
            }

            PrefabUtils.AddBasicComponents(tvPrefab, smartTVInfo.ClassID, smartTVInfo.TechType, LargeWorldEntity.CellLevel.Global);

            GameObject model = tvPrefab.transform.Find("GameObject").gameObject; //a child gameobject that contains the models
            if (model == null)
            {
                Debug.LogError("Model GameObject not found in TV prefab.");
                return;
            }
            model.transform.localPosition = new Vector3(0, -1, 0);

            float height = GetTotalHeight(model)/2;
            
            GameObject tvObj = tvPrefab.transform.Find("GameObject/tv").gameObject;
            AddMarmoSetShaderToGameObject(tvObj, new string[]{ "MARMO_SPECMAP" });


            foreach (Transform child in model.transform)
            {
                child.transform.localPosition = new Vector3(child.transform.localPosition.x, child.transform.localPosition.y + height, child.transform.localPosition.z);
            }

            GameObject planeObj = tvPrefab.transform.Find("GameObject/Plane").gameObject;
            AudioSource audioSource = planeObj.GetComponent<AudioSource>();
            audioSource.spatialBlend = 1f; //set spatial blend to 1 to make it 3D audio
            audioSource.spatialize = true; //enable spatialization
            //Material planeObjMat = AddMarmoSetShaderToGameObject(planeObj, new string[] { });

            GameObject canvasObj = tvPrefab.transform.Find("GameObject/Plane/Canvas").gameObject;
            canvasObj.AddComponent<AttachCameraToCanvas>();

            GameObject videoToggleObj = tvPrefab.transform.Find("GameObject/Plane/Canvas/Button").gameObject;
            videoToggleObj.AddComponent<VideoToggleControl>();
            videoToggleObj.AddComponent<BtnFade>();

            GameObject nextVideoObj = tvPrefab.transform.Find("GameObject/Plane/Canvas/NextVideo").gameObject;
            nextVideoObj.AddComponent<ChangeVideoBtn>();
            nextVideoObj.AddComponent<BtnFade>().fadeOutOnStart = true;

            GameObject previousVideoObj = tvPrefab.transform.Find("GameObject/Plane/Canvas/PreviousVideo").gameObject;
            previousVideoObj.AddComponent<ChangeVideoBtn>();
            previousVideoObj.AddComponent<BtnFade>().fadeOutOnStart = true;

            GameObject volumeSliderObj = tvPrefab.transform.Find("GameObject/Plane/Canvas/Slider").gameObject;
            volumeSliderObj.AddComponent<BtnFade>().fadeOutOnStart = true;

            ConstructableFlags constructableFlags = ConstructableFlags.Inside | ConstructableFlags.Rotatable | ConstructableFlags.Ground | ConstructableFlags.AllowedOnConstructable | ConstructableFlags.Submarine | ConstructableFlags.Outside;
            PrefabUtils.AddConstructable(tvPrefab, smartTVInfo.TechType, constructableFlags, model);

            CustomPrefab prefab = new CustomPrefab(smartTVInfo);
            prefab.SetGameObject(tvPrefab);

            string recipeJson = @"
            {
                ""craftAmount"": 1,
                ""Ingredients"": [
                    {
                        ""techType"": ""Titanium"",
                        ""amount"": 2
                    },
                    {
                        ""techType"": ""CopperWire"",
                        ""amount"": 1
                    }
                ]
            }";
            prefab.SetRecipeFromJson(recipeJson);
            prefab.SetPdaGroupCategory(TechGroup.InteriorModules, TechCategory.InteriorModule);
            prefab.Register();
        }

        public static Camera mainCamera;
        private float minDistance = 3f;
        private float maxDistance = 10f;

        private Material AddMarmoSetShaderToGameObject(GameObject obj, string[] shaderKeys)
        {
            MeshRenderer renderer = obj.GetComponent<MeshRenderer>();
            Material oldMat = renderer.sharedMaterial;
            Material newMat = new Material(MaterialUtils.Shaders.MarmosetUBER);
            newMat.color = oldMat.color;
            foreach (string key in shaderKeys)
            { 
                newMat.EnableKeyword(key);
            }
            newMat.globalIlluminationFlags = MaterialGlobalIlluminationFlags.RealtimeEmissive | MaterialGlobalIlluminationFlags.EmissiveIsBlack;
            renderer.sharedMaterial = newMat;
            return newMat;
        }

        void LateUpdate()
        {
            //audio makeshift
            if (mainCamera == null)
            { 
                mainCamera = FindObjectOfType<WaterSurfaceOnCamera>()?.gameObject.GetComponent<Camera>();
            }
            if (mainCamera != null)
            {
                AudioListener listener;
                if (!mainCamera.TryGetComponent<AudioListener>(out listener))
                { 
                    mainCamera.gameObject.AddComponent<AudioListener>();
                }
                VideoPlayer[] players = FindObjectsOfType<VideoPlayer>();
                if (players.Length > 0)
                { 
                    foreach (VideoPlayer player in players)
                    {
                        if (player.gameObject.activeInHierarchy && player.isPlaying)
                        {
                            float maxVolume = player.transform.Find("Canvas/Slider").GetComponent<Slider>().value;
                            float distance = Vector3.Distance(mainCamera.transform.position, player.transform.position);
                            if (distance < minDistance)
                            {
                                player.gameObject.GetComponent<AudioSource>().volume = Math.Min(maxVolume, 1f);
                            }
                            else
                            {
                                float newVolume = Mathf.Clamp01(1f - (distance - minDistance) / (maxDistance - minDistance)); //função linear que é y=1 em x=3 e y=0 em x=10
                                player.gameObject.GetComponent<AudioSource>().volume = Math.Min(maxVolume, newVolume);
                            }
                        }
                    }
                }
            }
        }

        public void UpdateVideosFolder(object sender, EventArgs e)
        {
            string folderPath = videosFolderPath;
            if (!Directory.Exists(folderPath))
            {
                Logger.LogError($"Folder not found: {folderPath}");
                return;
            }
            videos = Directory.GetFiles(folderPath, "*.*", SearchOption.TopDirectoryOnly)
                            .Where(file => file.EndsWith(
                            ".mp4", StringComparison.OrdinalIgnoreCase) ||
                            file.EndsWith(".avi", StringComparison.OrdinalIgnoreCase) ||
                            file.EndsWith(".mov", StringComparison.OrdinalIgnoreCase) ||
                            file.EndsWith(".mkv", StringComparison.OrdinalIgnoreCase)).ToArray();
            Debug.Log($"Videos found: {videos.Length} in {folderPath}");
            currentVideoIndex = 0;
        }

        public void NextVideo()
        {
            Debug.Log("NextVideo called");
            if (videos.Length == 0)
            {
                return;
            }
            currentVideoIndex++;
            if (currentVideoIndex > videos.Length - 1)
            {
                currentVideoIndex = 0;
            }
            ChangeVideo(videos[currentVideoIndex]);
        }

        public void PreviousVideo()
        {
            Debug.Log("PreviousVideo called");
            if (videos.Length == 0)
            {
                return;
            }
            currentVideoIndex--;
            if (currentVideoIndex < 0)
            { 
                currentVideoIndex = videos.Length - 1;
            }
            ChangeVideo(videos[currentVideoIndex]);
        }

        public void ChangeVideo(string path)
        {
            path = "file://" + path.Replace("\\", "/");
            Debug.Log($"Changing video to: {path}");
            VideoPlayer[] players = FindObjectsOfType<VideoPlayer>();
            foreach (VideoPlayer player in players)
            {
                bool isPlaying = player.isPlaying;
                player.source = VideoSource.Url;
                player.url = path;
                VideoToggleControl videoToggleControl = player.transform.Find("Canvas/Button").GetComponent<VideoToggleControl>();
                if (isPlaying)
                {
                    videoToggleControl.Pause();
                    videoToggleControl.Play();
                }
                else
                {
                    videoToggleControl.Play();
                    videoToggleControl.Pause();
                }
            }
        }

        public float GetTotalHeight(GameObject obj)
        {
            var renderers = obj.GetComponentsInChildren<Renderer>();
            if (renderers.Length == 0) return 0f;
            Bounds bounds = renderers[0].bounds;
            foreach (var rend in renderers)
            { 
                bounds.Encapsulate(rend.bounds);
            }
            return bounds.size.y;
        }

    }

}
