using UnityEngine;
using UnityEngine.UI;

namespace SmartTV
{

    class PowerBtn : MonoBehaviour
    {

        private bool isOn = true;
        private Material screenMaterial;
        private RawImage powerImage;
        private VideoToggleControl videoToggleControl;

        void Start()
        {
            Button buttonComponent = GetComponent<Button>();
            buttonComponent.onClick.RemoveAllListeners();
            buttonComponent.onClick.AddListener(this.Toggle);
            screenMaterial = transform.parent.parent.GetComponent<Renderer>().material;
            powerImage = transform.Find("RawImage").GetComponent<RawImage>();
            videoToggleControl = transform.parent.Find("Button").GetComponent<VideoToggleControl>();
        }

        private void Toggle()
        { 
            isOn = !isOn;
            screenMaterial.color = isOn ? Color.white : Color.black;
            foreach (Transform child in transform.parent)
            {
                if (child != this.transform)
                {
                    child.gameObject.SetActive(isOn);
                }
            }
            powerImage.color = isOn ? Color.white : Color.red;
            if (videoToggleControl.IsPlaying() && isOn)
            {
                videoToggleControl.Pause();
            }
        }
    
    }

}
