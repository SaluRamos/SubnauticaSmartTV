using Mono.Unix.Native;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace SmartTV
{

    class PowerBtn : MonoBehaviour
    {

        private bool isOn = true;
        private Material screenMaterial;

        void Start()
        {
            Button buttonComponent = GetComponent<Button>();
            buttonComponent.onClick.RemoveAllListeners();
            buttonComponent.onClick.AddListener(this.Toggle);
            screenMaterial = transform.parent.parent.GetComponent<Renderer>().material;
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
        }
    
    }

}
