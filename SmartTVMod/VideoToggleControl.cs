using System;
using System.Collections;
using System.Numerics;
using System.Security.Policy;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine.Video;

namespace SmartTV
{

    public class VideoToggleControl : MonoBehaviour
    {

        private VideoPlayer videoPlayer;
        private RawImage pauseImage;
        private RawImage playImage;
        private bool isPlaying = true;
        private int currentVideoIndex = 0;
        private string currentURL = "";

        public string GetCurrentURL()
        {
            return currentURL;
        }

        void Start()
        {
            Button buttonComponent = GetComponent<Button>();
            buttonComponent.onClick.RemoveAllListeners();
            buttonComponent.onClick.AddListener(this.Toggle);
            videoPlayer = transform.parent.parent.GetComponent<VideoPlayer>();
            videoPlayer.source = VideoSource.Url;
            videoPlayer.loopPointReached += OnVideoFinished;
            videoPlayer.isLooping = false;
            pauseImage = transform.Find("pause").GetComponent<RawImage>();
            playImage = transform.Find("play").GetComponent<RawImage>();
            NextVideo();
        }

        public bool IsPlaying()
        {
            return isPlaying;
        }

        public void Toggle()
        {
            if (videoPlayer.isPlaying)
            {
                Pause();
            }
            else
            {
                Play();
            }
        }

        public void Pause()
        {
            isPlaying = false;
            videoPlayer.Pause();
            playImage.enabled = true;
            pauseImage.enabled = false;
        }

        public void Play()
        {
            videoPlayer.Play();
            playImage.enabled = false;
            pauseImage.enabled = true;
            videoPlayer.Prepare();
            isPlaying = true;
        }

        private void OnVideoFinished(VideoPlayer source)
        {
            NextVideo();
        }

        public void NextVideo()
        {
            if (SmartTVMain.videos.Length == 0)
            {
                return;
            }
            currentVideoIndex++;
            if (currentVideoIndex > SmartTVMain.videos.Length - 1)
            {
                currentVideoIndex = 0;
            }
            ChangeVideo(SmartTVMain.videos[currentVideoIndex]);
        }

        public void PreviousVideo()
        {
            if (SmartTVMain.videos.Length == 0)
            {
                return;
            }
            currentVideoIndex--;
            if (currentVideoIndex < 0)
            {
                currentVideoIndex = SmartTVMain.videos.Length - 1;
            }
            ChangeVideo(SmartTVMain.videos[currentVideoIndex]);
        }

        private void ChangeVideo(string path)
        {
            currentURL = path;
            videoPlayer.url = "file://" + currentURL;
            Play();
        }

    }

}
