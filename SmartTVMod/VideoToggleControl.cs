using System.Collections;
using System.Numerics;
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

        public int GetCurrentVideoIndex()
        {
            return currentVideoIndex;
        }

        public VideoPlayer GetVideoPlayer()
        {
            return videoPlayer;
        }

        void Start()
        {
            Button buttonComponent = GetComponent<Button>();
            buttonComponent.onClick.RemoveAllListeners();
            buttonComponent.onClick.AddListener(this.Toggle);
            if (FindObjectOfType<EventSystem>() == null)
            {
                Debug.LogWarning("No EventSystem found, creating one.");
                GameObject es = new GameObject("EventSystem");
                es.AddComponent<EventSystem>();
                es.AddComponent<StandaloneInputModule>();
            }
            videoPlayer = transform.parent.parent.GetComponent<VideoPlayer>();
            videoPlayer.source = VideoSource.Url;
            videoPlayer.loopPointReached += OnVideoFinished;
            videoPlayer.isLooping = false;
            pauseImage = transform.Find("pause").GetComponent<RawImage>();
            playImage = transform.Find("play").GetComponent<RawImage>();
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
            isPlaying = true;
            videoPlayer.Play();
            playImage.enabled = false;
            pauseImage.enabled = true;
        }

        private void OnVideoFinished(VideoPlayer source)
        {
            NextVideo();
        }

        public void NextVideo()
        {
            Debug.Log("NextVideo called");
            if (SmartTVMain.instance.videos.Length == 0)
            {
                return;
            }
            currentVideoIndex++;
            if (currentVideoIndex > SmartTVMain.instance.videos.Length - 1)
            {
                currentVideoIndex = 0;
            }
            ChangeVideo(SmartTVMain.instance.videos[currentVideoIndex]);
        }

        public void PreviousVideo()
        {
            Debug.Log("PreviousVideo called");
            if (SmartTVMain.instance.videos.Length == 0)
            {
                return;
            }
            currentVideoIndex--;
            if (currentVideoIndex < 0)
            {
                currentVideoIndex = SmartTVMain.instance.videos.Length - 1;
            }
            ChangeVideo(SmartTVMain.instance.videos[currentVideoIndex]);
        }

        public void ChangeVideo(string path)
        {
            path = "file://" + path.Replace("\\", "/");
            videoPlayer.url = path;
            Pause();
            Play();
        }

    }

}
