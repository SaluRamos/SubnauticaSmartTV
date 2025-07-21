using UnityEngine;
using UnityEngine.Video;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.EventSystems;

namespace SmartTV
{

    public class VideoToggleControl : MonoBehaviour
    {

        private VideoPlayer videoPlayer;
        private RawImage pauseImage;
        private RawImage playImage;
        private bool isPlaying = true;
        private int currentVideoIndex = 0;

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
            pauseImage = transform.Find("pause").GetComponent<RawImage>();
            playImage = transform.Find("play").GetComponent<RawImage>();
        }

        public void SetCurrentVideoIndex(int index)
        {
            currentVideoIndex = index;
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
            Debug.Log($"Changing video to: {path}");
            VideoPlayer[] players = FindObjectsOfType<VideoPlayer>();
            foreach (VideoPlayer player in players)
            {
                Transform button = player.transform.Find("Canvas/Button");
                if (button == null) continue;

                VideoToggleControl videoToggleControl = button.GetComponent<VideoToggleControl>();
                if (videoToggleControl == null) continue;

                bool isPlaying = player.isPlaying;

                player.source = VideoSource.Url;
                player.url = path;
                player.isLooping = false;
                player.loopPointReached -= OnVideoFinished;
                player.loopPointReached += OnVideoFinished;

                // Optional: Update play/pause icon visuals
                Transform pauseIcon = button.Find("pause");
                Transform playIcon = button.Find("play");
                if (pauseIcon != null && playIcon != null)
                {
                    RawImage pauseImage = pauseIcon.GetComponent<RawImage>();
                    RawImage playImage = playIcon.GetComponent<RawImage>();
                    pauseImage.enabled = false;
                    playImage.enabled = true;
                }

                videoToggleControl.Pause(); // Ensure it's not playing
                videoToggleControl.Play();  // Now force play
            }
        }

    }

}
