using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace SmartTV
{

    public class AttachCameraToCanvas : MonoBehaviour
    {

        public Camera cameraRef;

        void Start()
        {
            if (cameraRef == null)
            {
                Camera[] cameras = FindObjectsOfType<Camera>();
                if (cameras.Length > 0)
                { 
                    cameraRef = cameras[0];
                }
            }
        }

        void LateUpdate()
        {
            if (gameObject.GetComponent<Canvas>().worldCamera == null && cameraRef != null)
            {
                cameraRef = FindObjectOfType<WaterSurfaceOnCamera>()?.gameObject.GetComponent<Camera>();
                if (cameraRef == null)
                {
                    return;
                }
                gameObject.GetComponent<Canvas>().worldCamera = cameraRef;
                Destroy(this);
            }
        }

    }

}
