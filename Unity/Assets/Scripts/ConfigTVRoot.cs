using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;
using UnityEngine.EventSystems;

public class ConfigTVRoot : MonoBehaviour
{

    void Start()
    {
        GameObject tvPrefab = GameObject.Find("TVRoot");
        GameObject canvasObj = tvPrefab.transform.Find("GameObject/Plane/Canvas").gameObject;
        canvasObj.AddComponent<AttachCameraToCanvas>();

        GameObject btnObj = tvPrefab.transform.Find("GameObject/Plane/Canvas/Button").gameObject;
        VideoToggleControl videoToggleControl = btnObj.AddComponent<VideoToggleControl>();

        if (FindObjectOfType<EventSystem>() == null)
        {
            GameObject es = new GameObject("EventSystem");
            es.AddComponent<EventSystem>();
            es.AddComponent<StandaloneInputModule>();
        }
    }
    
}
