using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using Nautilus.Assets;
using Nautilus.Assets.Gadgets;
using Nautilus.Assets.PrefabTemplates;
using Nautilus.Crafting;
using Nautilus.Extensions;
using Nautilus.Utility;
using System;
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

        public static string modFolder;
        public static string videosFolderPath;
        public static SmartTVMain instance;
        public static PrefabInfo smartTVInfo { get; } = PrefabInfo.WithTechType("SmartTV", "Smart TV", "A 60-inch Smart TV.");
        public static PrefabInfo bigSmartTVInfo { get; } = PrefabInfo.WithTechType("TheatherSmartTV", "Theather Smart TV", "A Theather Smart TV.");
        public static string[] videos;

        private void Awake()
        {
            instance = this;
            modFolder = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
            videosFolderPath = modFolder + "/Videos/";
            UpdateVideosFolder(null, null);
            gameObject.AddComponent<VideoThumbScrapper>();

            string bundlePath = modFolder + "/60insmarttv";
            AssetBundle bundle = AssetBundle.LoadFromFile(bundlePath);
            if (bundle == null)
            {
                Logger.LogError("AssetBundle not found at: " + modFolder);
                return;
            }

            Sprite icon = bundle.LoadAsset<Sprite>("TVSprite");

            GameObject smartTvPrefab = bundle.LoadAsset<GameObject>("TVRoot");
            if (smartTvPrefab == null)
            {
                Logger.LogError("base TV prefab not found in AssetBundle.");
                return;
            }
            ConfigTVPrefab(ref smartTvPrefab);

            GameObject theatherTvPrefab = bundle.LoadAsset<GameObject>("TheaterTVRoot");
            if (theatherTvPrefab == null)
            {
                Logger.LogError("base TV prefab not found in AssetBundle.");
                return;
            }
            ConfigTVPrefab(ref theatherTvPrefab);
            theatherTvPrefab.transform.localScale *= 1.5f;

            // --- USING NAUTILUS ---

            string smartTVRecipeJson = @"
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

            string theaterRecipeJson = @"
            {
                ""craftAmount"": 1,
                ""Ingredients"": [
                    {
                        ""techType"": ""Titanium"",
                        ""amount"": 3
                    },
                    {
                        ""techType"": ""CopperWire"",
                        ""amount"": 1
                    }
                ]
            }";

            CreatePrefab(smartTVInfo, smartTvPrefab, smartTVRecipeJson, icon);
            CreatePrefab(bigSmartTVInfo, theatherTvPrefab, theaterRecipeJson, icon);
        }

        private void ConfigTVPrefab(ref GameObject prefab)
        {
            GameObject model = prefab.transform.Find("GameObject").gameObject; //a child gameobject that contains the models
            if (model == null)
            {
                Debug.LogError("Model GameObject not found in TV prefab.");
            }
            model.transform.localPosition = new Vector3(0, -1, 0);

            float height = GetTotalHeight(model) / 2;
            GameObject tvObj = prefab.transform.Find("GameObject/tv").gameObject;
            AddMarmoSetShaderToGameObject(tvObj, new string[] { "MARMO_SPECMAP" });

            foreach (Transform child in model.transform)
            {
                child.transform.localPosition = new Vector3(child.transform.localPosition.x, child.transform.localPosition.y + height, child.transform.localPosition.z);
            }

            GameObject planeObj = prefab.transform.Find("GameObject/Plane").gameObject;
            Renderer screenRenderer = planeObj.GetComponent<Renderer>();
            Material screenMat = screenRenderer.sharedMaterial;

            AudioSource audioSource = planeObj.GetComponent<AudioSource>();
            audioSource.spatialBlend = 1f; //set spatial blend to 1 to make it 3D audio
            audioSource.spatialize = true; //enable spatialization

            GameObject canvasObj = prefab.transform.Find("GameObject/Plane/Canvas").gameObject;
            canvasObj.AddComponent<AttachCameraToCanvas>();

            GameObject videoToggleObj = prefab.transform.Find("GameObject/Plane/Canvas/Button").gameObject;
            videoToggleObj.AddComponent<VideoToggleControl>();
            videoToggleObj.AddComponent<BtnFade>().fadeOutOnStart = true;

            GameObject nextVideoObj = prefab.transform.Find("GameObject/Plane/Canvas/NextVideo").gameObject;
            nextVideoObj.AddComponent<ChangeVideoBtn>();
            nextVideoObj.AddComponent<BtnFade>().fadeOutOnStart = true;

            GameObject previousVideoObj = prefab.transform.Find("GameObject/Plane/Canvas/PreviousVideo").gameObject;
            previousVideoObj.AddComponent<ChangeVideoBtn>();
            previousVideoObj.AddComponent<BtnFade>().fadeOutOnStart = true;

            GameObject volumeSliderObj = prefab.transform.Find("GameObject/Plane/Canvas/Slider").gameObject;
            volumeSliderObj.AddComponent<BtnFade>().fadeOutOnStart = true;

            GameObject powerBtnObj = prefab.transform.Find("GameObject/Plane/Canvas/Power").gameObject;
            powerBtnObj.AddComponent<PowerBtn>();
            powerBtnObj.AddComponent<BtnFade>().fadeOutOnStart = true;

            GameObject seekSliderObj = prefab.transform.Find("GameObject/Plane/Canvas/SeekSlider").gameObject;
            seekSliderObj.gameObject.AddComponent<VideoScrubber>();
            BtnFade seekSliderFade = seekSliderObj.gameObject.AddComponent<BtnFade>();
            seekSliderFade.fadeOutOnStart = true;
            seekSliderFade.fadeOutWhenLookingTooMuch = false;
        }

        private void CreatePrefab(PrefabInfo info, GameObject prefab, string recipeJson, Sprite icon)
        {
            if (prefab == null)
            {
                Logger.LogError($"Prefab for {info.ClassID} is null.");
            }

            PrefabUtils.AddBasicComponents(prefab, info.ClassID, info.TechType, LargeWorldEntity.CellLevel.Global);

            GameObject model = prefab.transform.Find("GameObject").gameObject;
            if (model == null)
            { 
                Logger.LogError($"Model GameObject not found in prefab {info.ClassID}.");
            }

            ConstructableFlags constructableFlags = ConstructableFlags.Inside | ConstructableFlags.Rotatable | ConstructableFlags.Ground | ConstructableFlags.AllowedOnConstructable | ConstructableFlags.Submarine | ConstructableFlags.Outside;

            PrefabUtils.AddConstructable(prefab, info.TechType, constructableFlags, model);

            CustomPrefab customPrefab = new CustomPrefab(info);

            if (icon != null)
            {
                info.WithIcon(icon);
            }
            else
            { 
                Logger.LogError("TV icon not found in AssetBundle.");
            }

            customPrefab.SetGameObject(prefab);
            customPrefab.SetRecipeFromJson(recipeJson);
            customPrefab.SetPdaGroupCategory(TechGroup.InteriorModules, TechCategory.InteriorModule);
            customPrefab.SetUnlock(TechType.Titanium); //unlock with Titanium
            customPrefab.Register();
        }

        public static Camera mainCamera;
        private float minDistance = 3f;
        private float maxDistance = 20f;

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
            newMat.SetColor("_Color", Color.black);
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
                                float newVolume = Mathf.Clamp01(1f - (distance - minDistance) / (maxDistance - minDistance)); //linear function is y=1 when x=3 and y=0 when x=10
                                player.gameObject.GetComponent<AudioSource>().volume = Math.Min(maxVolume, newVolume);
                            }
                        }
                    }
                }
            }

            if (FindObjectOfType<EventSystem>() == null)
            {
                Debug.LogWarning("No EventSystem found, creating one.");
                GameObject es = new GameObject("EventSystem");
                es.AddComponent<EventSystem>();
                es.AddComponent<StandaloneInputModule>();
            }
        }

        public void UpdateVideosFolder(object sender, EventArgs e)
        {
            string folderPath = videosFolderPath;
            if (!Directory.Exists(folderPath))
            {
                Debug.LogError($"Folder not found: {folderPath}");
                return;
            }
            videos = Directory.GetFiles(folderPath, "*.*", SearchOption.TopDirectoryOnly)
                            .Where(file => file.EndsWith(
                            ".mp4", StringComparison.OrdinalIgnoreCase) ||
                            file.EndsWith(".avi", StringComparison.OrdinalIgnoreCase) ||
                            file.EndsWith(".mov", StringComparison.OrdinalIgnoreCase) ||
                            file.EndsWith(".mkv", StringComparison.OrdinalIgnoreCase)).ToArray();
            for (int index = 0; index < videos.Length; index++)
            {
                videos[index] = videos[index].Replace("\\", "/");
            }
            Debug.Log($"Videos found: {videos.Length} in {folderPath}");
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
