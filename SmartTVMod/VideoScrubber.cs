using System;
using System.Diagnostics;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine.Video;
using static ScreenshotManager;

namespace SmartTV
{
    public class VideoScrubber : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
    {

        private VideoPlayer videoPlayer;
        private VideoToggleControl videoToggleControl;
        private Slider slider;
        private Text timeText;
        private RawImage hoverThumbnail;
        private bool isDragging = false;

        void Start()
        {
            videoPlayer = transform.parent.parent.GetComponent<VideoPlayer>();
            slider = GetComponent<Slider>();
            timeText = transform.Find("ScrubTime").GetComponent<Text>();
            videoToggleControl = transform.parent.Find("Button").GetComponent<VideoToggleControl>();
            if (videoToggleControl == null)
            {
                UnityEngine.Debug.LogError("[VideoScrubber] Could not locate VideoToggleControl component.");
            }

            Transform thumb = transform.Find("Handle Slide Area/Handle/RawImage");
            if (thumb != null)
            {
                hoverThumbnail = thumb.GetComponent<RawImage>();
                hoverThumbnail.color = Color.white;
            }
            else
            {
                UnityEngine.Debug.LogError("[Thumbnail] Could not locate ThumbnailPreview.");
            }
        }

        private double timeToUpdateSlider = 1f;
        private long lastSliderUpdate = 0;

        void LateUpdate()
        {
            if (videoToggleControl.IsPlaying())
            {
                UpdateTimeDisplay();
                long actualTimestamp = Stopwatch.GetTimestamp();
                long elapsedTicks = actualTimestamp - lastSliderUpdate;
                double elapsedSeconds = (double)elapsedTicks / Stopwatch.Frequency;
                if (!isDragging && elapsedSeconds > timeToUpdateSlider)
                {
                    lastSliderUpdate = Stopwatch.GetTimestamp();
                    float normalizedTime = (float)(videoPlayer.time / videoPlayer.length);
                    slider.value = normalizedTime;
                }
                hoverThumbnail.texture = VideoThumbScrapper.GetFrame(videoToggleControl.GetCurrentURL(), slider.value);
            }
        }

        private void UpdateTimeDisplay()
        {
            string Format(double sec)
            {
                var t = TimeSpan.FromSeconds(sec);
                return t.Hours > 0 ? $"{t.Hours:D2}:{t.Minutes:D2}:{t.Seconds:D2}" : $"{t.Minutes:D2}:{t.Seconds:D2}";
            }
            timeText.text = $"{Format(videoPlayer.time)} / {Format(videoPlayer.length)}";
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            isDragging = true;
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            isDragging = false;
            videoPlayer.time = slider.value * videoPlayer.length;
            hoverThumbnail.texture = VideoThumbScrapper.GetFrame(videoToggleControl.GetCurrentURL(), slider.value);
        }

    }
}