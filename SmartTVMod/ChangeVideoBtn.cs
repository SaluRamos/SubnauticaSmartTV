using SmartTV;
using System;
using System.Collections.Generic;
using System.Diagnostics;
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

        void Start()
        {
            videoToggleControl = transform.parent.Find("Button").GetComponent<VideoToggleControl>();
            if (videoToggleControl == null)
            {
                UnityEngine.Debug.LogError("VideoToggleControl is null at ChangeVideoBtn");
            }
            Button buttonComponent = GetComponent<Button>();
            buttonComponent.onClick.AddListener(ChangeVideo);
        }

        private long lastClick;
        private double minTimeBetweenClicks = 0.3; // 300 milliseconds

        private void ChangeVideo()
        {
            long actualTimestamp = Stopwatch.GetTimestamp();
            long elapsedTicks = actualTimestamp - lastClick;
            double elapsedSeconds = (double)elapsedTicks / Stopwatch.Frequency;
            if (elapsedSeconds < minTimeBetweenClicks)
            {
                return; // Ignore clicks that are too close together
            }
            lastClick = actualTimestamp;
            if (gameObject.name.Contains("Next"))
            {
                videoToggleControl.NextVideo();
            }
            else
            {
                videoToggleControl.PreviousVideo();
            }
        }

    }
}
