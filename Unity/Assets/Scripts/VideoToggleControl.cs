using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;
using System.Collections;
using UnityEngine.EventSystems;

public class VideoToggleControl : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{

    private CanvasGroup targetGroup;
    private Coroutine fadeRoutine;

    private VideoPlayer videoPlayer;
    private RawImage pauseImage;
    private RawImage playImage;

    public void OnPointerEnter(PointerEventData eventData)
    {
        UpdateVisibility(true);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        UpdateVisibility(false);
    }

    private void UpdateVisibility(bool fadeIn)
    {
        if (targetGroup == null)
        { 
            targetGroup = GetComponent<CanvasGroup>();
        }
        if (fadeRoutine != null)
        { 
            StopCoroutine(fadeRoutine);
        }
        fadeRoutine = StartCoroutine(FadeCanvasGroup(fadeIn));
    }

    private IEnumerator FadeCanvasGroup(bool fadeIn)
    {
        float start = targetGroup.alpha;
        float targetValue = fadeIn ? 1f : 0f;
        float t = 0f;
        float duration = 0.15f;
        while (t < 1f)
        {
            t += Time.unscaledDeltaTime / duration;
            targetGroup.alpha = Mathf.Lerp(start, targetValue, t);
            yield return null;
        }
        targetGroup.alpha = targetValue;
    }

    public void Toggle()
    {
        Debug.Log("toggle video pause/play");
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
            videoPlayer.Pause();
            playImage.enabled = true;
            pauseImage.enabled = false;
        }
        else
        {
            videoPlayer.Play();
            playImage.enabled = false;
            pauseImage.enabled = true;
        }
    }
}
