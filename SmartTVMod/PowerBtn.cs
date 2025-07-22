using UnityEngine;
using UnityEngine.UI;

namespace SmartTV
{

    class PowerBtn : MonoBehaviour
    {

        private bool isOn = true;
        private Material screenMaterial;
        private VideoToggleControl videoToggleControl;

        void Start()
        {
            Button buttonComponent = GetComponent<Button>();
            buttonComponent.onClick.RemoveAllListeners();
            buttonComponent.onClick.AddListener(this.Toggle);
            screenMaterial = transform.parent.parent.GetComponent<Renderer>().material;
            videoToggleControl = transform.parent.Find("Button").GetComponent<VideoToggleControl>();
            Toggle(); //turn off the tv on start
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

            if (videoToggleControl != null)
            {
                if (!isOn)
                {
                    videoToggleControl.Pause();
                }
                else 
                {
                    videoToggleControl.Play();
                }
            }
        }
    }
}
                    
                
           
