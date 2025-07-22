using FFMediaToolkit;
using FFMediaToolkit.Decoding;
using FFMediaToolkit.Graphics;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;
using Valve.VR;

namespace SmartTV
{

    //install the package with: Install-Package FFMediaToolkit
    public class VideoThumbScrapper : MonoBehaviour
    {

        private static bool loadedFFMPEG = false;
        private static Dictionary<string, Video> map = new Dictionary<string, Video>();
        private static int TargetWidth = 256;
        private static int TargetHeight = 144;
        private static int TotalFramesInCache = 50;

        private class Video
        {
            public MediaFile mediaFile;
            public Texture2D[] texs = new Texture2D[TotalFramesInCache];
        }

        void Start()
        { 
            if (!loadedFFMPEG)
            { 
                FFmpegLoader.FFmpegPath = SmartTVMain.modFolder;
                loadedFFMPEG = true;
            }
            foreach (string url in SmartTVMain.videos)
            {
                StartCoroutine(LoadVideo(url));
            }
        }

        private IEnumerator LoadVideo(string url)
        {
            Debug.LogWarning($"Start loading {url}");
            map[url] = new Video();
            try
            {
                var options = new MediaOptions
                {
                    TargetVideoSize = new Size(TargetWidth, TargetHeight),
                    VideoPixelFormat = ImagePixelFormat.Rgba32
                };
                map[url].mediaFile = MediaFile.Open(url, options);
            }
            catch (Exception e)
            {
                Debug.LogError($"Falha ao abrir o arquivo de mídia '{url}': {e.Message}");
                map[url].mediaFile = null;
                yield break;
            }

            for (int index = 0; index < TotalFramesInCache; index++)
            {
                LoadFrame(url, index);
                yield return null;
            }
        }

        private async Task LoadVideoAsync(string url)
        {
            Debug.LogWarning($"Start loading {url}");
            map[url] = new Video();
            await Task.Run(() =>
            {
                try
                {
                    var options = new MediaOptions
                    {
                        TargetVideoSize = new Size(TargetWidth, TargetHeight),
                        VideoPixelFormat = ImagePixelFormat.Rgba32
                    };
                    map[url].mediaFile = MediaFile.Open(url, options);
                }
                catch (Exception e)
                {
                    Debug.LogError($"Falha ao abrir o arquivo de mídia '{url}': {e.Message}");
                    map[url].mediaFile = null;
                    return;
                }
            });

            if (map[url].mediaFile == null)
                return;

            for (int index = 0; index < TotalFramesInCache; index++)
            {
                int i = index;
                await Task.Run(() => LoadFrame(url, i));
            }
        }

        private void LoadFrame(string url, int index)
        {
            double time = (index / (double)TotalFramesInCache) * map[url].mediaFile.Info.Duration.TotalSeconds;
            Debug.Log($"Setting video thumb {index} with time {time}");
            var ts = TimeSpan.FromSeconds(time);

            if (map[url].mediaFile.Video.TryGetFrame(ts, out ImageData frame))
            {
                byte[] RgbaPixels = frame.Data.ToArray();
                if (map[url].texs[index] == null) //no need to verify texs size because it is fixed size
                {
                    map[url].texs[index] = new Texture2D(TargetWidth, TargetHeight, TextureFormat.RGBA32, false);
                }
                map[url].texs[index].LoadRawTextureData(RgbaPixels);
                map[url].texs[index].Apply();
            }
        }

        public static Texture2D GetFrame(string url, double time)
        {
            if (time < 0 || time > 1)
            {
                Debug.LogError($"Time {time} is out of bounds. Must be between 0 and 1.");
                return null;
            }
            if (!map.ContainsKey(url))
            {
                Debug.LogError($"Video not loaded: {url}");
                return null;
            }
            Video video = map[url];
            int index = (int) Math.Max(0, Math.Round(time * TotalFramesInCache) - 1);
            if (video.texs.Length - 1 < index)
            { 
                Debug.LogError($"Index {index} out of bounds for video {url}. Total frames: {video.texs.Length}");
                return null;
            }
            return video.texs[index];
        }

    }
}
