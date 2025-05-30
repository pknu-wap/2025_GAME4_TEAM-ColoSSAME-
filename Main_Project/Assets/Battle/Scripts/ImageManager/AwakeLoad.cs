using System;
using UnityEngine;

namespace Battle.Scripts.ImageManager
{
    public class AwakeLoad : MonoBehaviour
    {
        public TransparentScreenshot TransparentScreenshot;
        // Start is called before the first frame update
        void Awake()
        {
            TransparentScreenshot.GetComponent<TransparentScreenshot>();
        }

        private void Start()
        {
            TransparentScreenshot.LoadAllSprites();
        }

        // Update is called once per frame
        void Update()
        {
        
        }
    }
}
