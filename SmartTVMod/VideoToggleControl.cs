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
        private bool isPlaying = false;

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
        }

        public bool IsPlaying()
        {
            return isPlaying;
        }

        public void Toggle()
        {
            if (videoPlayer == null)
            {
                videoPlayer = transform.parent.parent.GetComponent<VideoPlayer>();
            }
            if (pauseImage == null)
            {
                pauseImage = transform.Find("pause").GetComponent<RawImage>();
            }
            if (playImage == null)
            {
                playImage = transform.Find("play").GetComponent<RawImage>();
            }

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

    }

}
