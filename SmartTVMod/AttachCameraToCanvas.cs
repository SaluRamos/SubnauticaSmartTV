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

        void LateUpdate()
        {
            if (gameObject.GetComponent<Canvas>().worldCamera == null)
            {
                cameraRef = FindObjectOfType<WaterSurfaceOnCamera>()?.gameObject.GetComponent<Camera>();
                if (cameraRef == null) //for menu
                {
                    return;
                }
                gameObject.GetComponent<Canvas>().worldCamera = cameraRef;
                Destroy(this);
            }
        }

    }

}
