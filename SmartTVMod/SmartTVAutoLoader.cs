using UnityEngine;
using UnityEngine.Video;
using System.Collections;
using System.IO;

namespace SmartTV
{
    public class SmartTVAutoLoader : MonoBehaviour
    {
        
        void Start()
        {
            var vids = SmartTVMain.instance.videos;
            if (vids == null || vids.Length == 0)
            {
                Debug.LogWarning("[AutoLoader] No videos found.");
                return;
            }

            string path = vids[0];
            if (!File.Exists(path))
            {
                Debug.LogError("[AutoLoader] First video file not found: " + path);
                return;
            }

            VideoPlayer vp = GetComponent<VideoPlayer>();
            if (vp == null)
            {
                Debug.LogError("[AutoLoader] VideoPlayer not found on this GameObject.");
                return;
            }

            vp.source = VideoSource.Url;
            vp.url = "file://" + path.Replace("\\", "/");
            vp.isLooping = false;

            Debug.Log("[AutoLoader] Preparing: " + vp.url);

            vp.prepareCompleted += (player) =>
            {
                Debug.Log("[AutoLoader] Video ready to play: " + player.url);
            };

            vp.errorReceived += (player, message) =>
            {
                Debug.LogError("[AutoLoader] Error loading video: " + message);
            };

            vp.Prepare();
        }

    }
}
