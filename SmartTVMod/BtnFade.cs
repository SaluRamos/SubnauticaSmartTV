using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.EventSystems;

namespace SmartTV
{
    public class BtnFade : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {

        private CanvasGroup targetGroup;
        private Coroutine fadeRoutine;
        public bool fadeOutOnStart = false;

        void Start()
        {
            if (!TryGetComponent<CanvasGroup>(out targetGroup))
            {
                targetGroup = gameObject.AddComponent<CanvasGroup>();
                targetGroup.alpha = 1f;
                targetGroup.interactable = true;
                targetGroup.blocksRaycasts = true;
                targetGroup.ignoreParentGroups = false;
            }
            if (fadeOutOnStart)
            {
                OnPointerExit(null);
            }
        }

        private float minDistanceToInteract = 3f;

        public void OnPointerEnter(PointerEventData eventData)
        {
            float distance = Vector3.Distance(SmartTVMain.mainCamera.transform.position, transform.position);
            if (distance > minDistanceToInteract)
            {
                return;
            }
            UpdateVisibility(true);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            float distance = Vector3.Distance(Camera.main.transform.position, transform.position);
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

    }
}
