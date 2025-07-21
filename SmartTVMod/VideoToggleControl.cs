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

    }

}
