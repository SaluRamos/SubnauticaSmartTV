using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AttachCameraToCanvas : MonoBehaviour
{

    public Camera cameraRef;

    void Start()
    {
        if (cameraRef == null)
        {
            cameraRef = FindObjectsOfType<Camera>()[0];
        }
    }

    void LateUpdate()
    {
        if (gameObject.GetComponent<Canvas>().worldCamera == null && cameraRef != null)
        {
            gameObject.GetComponent<Canvas>().worldCamera = cameraRef;
            Destroy(this);
        }    
    }

}
