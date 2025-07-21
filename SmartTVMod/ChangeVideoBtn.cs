using SmartTV;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace SmartTV
{
    public class ChangeVideoBtn : MonoBehaviour
    {

        private VideoToggleControl videoToggleControl;
        public UnityAction action;

        void Start()
        {
            videoToggleControl = transform.parent.parent.GetComponent<VideoToggleControl>();
            Button buttonComponent = GetComponent<Button>();
            buttonComponent.onClick.RemoveAllListeners();
            if (gameObject.name.Contains("Next"))
            { 
                action = videoToggleControl.NextVideo;
            }
            else
            {
                action = videoToggleControl.PreviousVideo;
            }
            buttonComponent.onClick.AddListener(action);
            if (FindObjectOfType<EventSystem>() == null)
            {
                Debug.LogWarning("No EventSystem found, creating one.");
                GameObject es = new GameObject("EventSystem");
                es.AddComponent<EventSystem>();
                es.AddComponent<StandaloneInputModule>();
            }
        }

    }
}
