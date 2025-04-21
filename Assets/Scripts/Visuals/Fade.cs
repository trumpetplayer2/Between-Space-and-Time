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
        bool fadeIn = false;

        private void Start()
        {
            c = i.color;
        }

        // Update is called once per frame
        void Update()
        {
            if (fadeIn) {
                timer += Time.deltaTime;
                c.a = (timer / fadeTime);
                i.color = c;
            }
            else
            {
                timer += Time.deltaTime;
                c.a = 1 - (timer / fadeTime);
                i.color = c;
            }
        }

        public void toggleFade(bool state)
        {
            fadeIn = state;
            timer = 0;
        }
    }
}
