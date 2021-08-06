using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

namespace Assets
{
    [RequireComponent(typeof(RawImage))]
    class TGALoader_TestComponent : MonoBehaviour
    {

        [SerializeField]
        public string pathToTGA;
        
        private void Start()
        {
            if (string.IsNullOrEmpty(pathToTGA))
            {
                Debug.Log("image directory not specified");
            }

            float timeStart = Time.realtimeSinceStartup;
            var tex = OpenMetaverse.Imaging.LoadTGAClass.LoadTGA(pathToTGA);

            float timeEnd = Time.realtimeSinceStartup;
            if (tex == null)
            {
                Debug.Log("reading of TGA at path " + pathToTGA + " failed");
            }
            else
            {
                Debug.Log("reading of TGA at path " + pathToTGA + " SUCCESS! \nTook: " + (timeEnd-timeStart) + "seconds");
                gameObject.GetComponent<RawImage>().texture = tex;
            }

        }


    }
}
