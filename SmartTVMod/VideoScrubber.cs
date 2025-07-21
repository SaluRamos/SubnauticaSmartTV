using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine.Video;
using static ScreenshotManager;

namespace SmartTV
{
    public class VideoScrubber : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
    {

        private VideoThumbScrapper thumbScrapper;
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
                Debug.LogError("[VideoScrubber] Could not locate VideoToggleControl component.");
            }

            Transform thumb = transform.Find("Handle Slide Area/Handle/RawImage");
            if (thumb != null)
            {
                hoverThumbnail = thumb.GetComponent<RawImage>();
                hoverThumbnail.color = Color.white;
            }
            else
            {
                Debug.LogError("[Thumbnail] Could not locate ThumbnailPreview.");
            }
            thumbScrapper = new VideoThumbScrapper(videoToggleControl);
        }

        void LateUpdate()
        {
            UpdateTimeDisplay();

            if (!isDragging)
            {
                slider.value = (float)(videoPlayer.time / videoPlayer.length);
            }

            if (isDragging)
            {
                double slideTime = slider.value * videoPlayer.length;
                thumbScrapper.GetFrame(slideTime, hoverThumbnail);
            }
            else
            { 
                hoverThumbnail.texture = videoPlayer.texture;
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
        }

        void OnDestroy()
        {
            thumbScrapper.Dispose();
        }

    }
}