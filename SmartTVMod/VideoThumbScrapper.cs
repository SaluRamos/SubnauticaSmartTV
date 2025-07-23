using FFMediaToolkit;
using FFMediaToolkit.Decoding;
using FFMediaToolkit.Graphics;
using FFmpeg.AutoGen;
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
        private static int TargetWidth = 196;
        private static int TargetHeight = 110;
        private static int TotalFramesInCache = 30;

        private class Video
        {
            public MediaFile mediaFile;
            public Texture2D[] texs = new Texture2D[TotalFramesInCache];
        }

        void Start()
        { 
            if (!loadedFFMPEG)
            {
                ffmpeg.RootPath = SmartTVMain.modFolder;
                FFmpegLoader.FFmpegPath = SmartTVMain.modFolder;
                loadedFFMPEG = true;
            }
            StartCoroutine(LoadAllVideosOneByOne());
        }

        private IEnumerator LoadAllVideosOneByOne()
        { 
            foreach (string url in SmartTVMain.videos)
            {
                LoadVideo(url);
            }
            yield break;
        }

        private void LoadVideo(string url)
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
                using (var mediaFile = MediaFile.Open(url, options))
                {
                    map[url].mediaFile = mediaFile;
                    var totalFrameCount = mediaFile.Video.Info.NumberOfFrames;
                    if (totalFrameCount.GetValueOrDefault() <= 0) return; // no frames in video
                    var framesToCapture = new HashSet<int>();
                    for (int i = 0; i < TotalFramesInCache; i++)
                    {
                        int frameIndex = (int)((i / (double)TotalFramesInCache) * totalFrameCount.Value);
                        framesToCapture.Add(frameIndex);
                    }
                    int currentFrame = 0;
                    int index = 0;

                    while (framesToCapture.Count > 0 && mediaFile.Video.TryGetNextFrame(out var frame))
                    {
                        if (framesToCapture.Contains(currentFrame))
                        {
                            Debug.Log($"Loading frame {index}");
                            framesToCapture.Remove(currentFrame);
                            byte[] RgbaPixels = frame.Data.ToArray();
                            if (map[url].texs[index] == null) //no need to verify texs size because it is fixed size
                            {
                                map[url].texs[index] = new Texture2D(TargetWidth, TargetHeight, TextureFormat.RGBA32, false);
                            }
                            map[url].texs[index].LoadRawTextureData(RgbaPixels);
                            map[url].texs[index].Apply();
                            index++;
                        }
                        currentFrame++;
                    }
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"Failed '{url}': {e.Message}");
                map[url] = null;
            }
        }

        public static Texture2D GetFrame(string url, double time)
        {
            int index = 0;
            try
            {
                if (double.IsNaN(time))
                {
                    return null;
                }
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
                index = (int)Math.Max(0, Math.Round(time * TotalFramesInCache) - 1);
                if (video.texs.Length - 1 < index)
                {
                    Debug.LogError($"Index {index} out of bounds for video {url}. Total frames: {video.texs.Length}");
                    return null;
                }
                return video.texs[index];
            }
            catch (Exception e)
            { 
                Debug.LogError($"Error getting frame for {url}, index {index} at time {time}: {e.Message}");
            }
            return null;
        }

    }
}
