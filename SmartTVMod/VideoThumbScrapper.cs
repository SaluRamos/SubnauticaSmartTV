using FFMediaToolkit;
using FFMediaToolkit.Decoding;
using FFMediaToolkit.Graphics;
using FFMediaToolkit.Graphics;
using System;
using System.Drawing;
using System.Threading.Tasks;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
using Valve.VR;

namespace SmartTV
{

    //install the package with: Install-Package FFMediaToolkit
    class VideoThumbScrapper
    {

        private VideoToggleControl videoToggleControl;
        private double lastFrameTime = 0;
        private MediaFile mediaFile;
        private string lastUrl = "";

        private Texture2D lastTex;

        public VideoThumbScrapper(VideoToggleControl videoToggleControl)
        {
            FFmpegLoader.FFmpegPath = SmartTVMain.modFolder;
            this.videoToggleControl = videoToggleControl;
        }

        private Texture2D[] texs;

        private async Task UpdateUrlAsync()
        {
            string url = SmartTVMain.instance.videos[videoToggleControl.GetCurrentVideoIndex()].Replace("\\", "/");
            if (url != lastUrl)
            {
                //int secondsInVideo = (int) Math.Floor(videoToggleControl.GetVideoPlayer().length);

                lastUrl = url;
                if (mediaFile != null) mediaFile.Dispose();
                try
                {
                    var options = new MediaOptions
                    {
                        TargetVideoSize = new Size(TargetWidth, TargetHeight),
                        VideoPixelFormat = ImagePixelFormat.Rgba32
                    };
                    mediaFile = await Task.Run(() => MediaFile.Open(url, options));
                }
                catch (Exception e)
                {
                    Debug.LogError($"[SmartTVMod] Falha ao abrir o arquivo de mídia '{url}': {e.Message}");
                    mediaFile = null;
                }
            }
        }

        private bool isProcessing = false;
        private volatile int latestRequestId = 0;
        private struct FrameDataResult
        {
            public bool Success;
            public byte[] RgbaPixels;
        }
        private const int TargetWidth = 256;
        private const int TargetHeight = 144;

        public async void GetFrame(double time, RawImage hoverThumbnail)
        {

            if (isProcessing) return;

            if (Math.Abs(time - lastFrameTime) < 0.1)
            {
                return;
            }

            int currentRequestId = ++latestRequestId;
            isProcessing = true;

            try
            {
                await UpdateUrlAsync();
                if (mediaFile == null) return;
                var ts = TimeSpan.FromSeconds(time);
                FrameDataResult result = await Task.Run(() =>
                {
                    if (mediaFile.Video.TryGetFrame(ts, out ImageData frame))
                    {
                        return new FrameDataResult { Success = true, RgbaPixels = frame.Data.ToArray() };
                    }
                    return new FrameDataResult { Success = false };
                });

                if (currentRequestId != latestRequestId)
                {
                    return;
                }

                if (result.Success)
                {
                    if (lastTex == null)
                    {
                        lastTex = new Texture2D(TargetWidth, TargetHeight, TextureFormat.RGBA32, false);
                    }

                    lastTex.LoadRawTextureData(result.RgbaPixels);
                    lastTex.Apply();

                    lastFrameTime = time;
                    hoverThumbnail.texture = lastTex;
                }
            }
            finally
            {
                isProcessing = false;
            }
        }

        public void Dispose()
        {
            latestRequestId++;
            if (mediaFile != null)
            {
                mediaFile.Dispose();
            }
            if (lastTex != null)
            {
                UnityEngine.Object.Destroy(lastTex);
            }
        }

    }

}
