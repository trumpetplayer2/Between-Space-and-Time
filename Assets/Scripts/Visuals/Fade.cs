using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace tp2
{
    public class Fade : MonoBehaviour
    {
        public Image i;
        public float fadeTime = 1f;
        float timer = 0;
        Color c;

        private void Start()
        {
            c = i.color;
        }

        // Update is called once per frame
        void Update()
        {
            timer += Time.deltaTime;
            c.a = 1 - (timer / fadeTime);
            i.color = c;
        }
    }
}
