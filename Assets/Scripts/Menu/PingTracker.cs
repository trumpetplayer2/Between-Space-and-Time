using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

namespace tp2
{
    public class PingTracker : MonoBehaviour
    {
        public static PingTracker instance;
        public TextMeshProUGUI text;
        float[] pings = new float[0];
        float timeSinceLast = 0f;
        float updateIncrement = 1f;
        private void Start()
        {
            instance = this;
        }

        private void OnDestroy()
        {
            if (instance == this)
            {
                instance = null;
            }
        }

        public void updatePing(float ping)
        {
            float[] tempPings = new float[pings.Length + 1];
            for(int i = 0; i < pings.Length; i++)
            {
                tempPings[i] = pings[i];
            }
            tempPings[pings.Length] = ping;
            pings = tempPings;
        }

        public void Update()
        {
            if(timeSinceLast >= updateIncrement)
            {
                float total = 0;
                foreach(float f in pings){
                    total += f;
                }
                float avg = total / pings.Length;
                text.text = Mathf.Round(avg) + " ms (" + pings.Length + ") pings";
                timeSinceLast = 0f;
                pings = new float[0];
            }
            else
            {
                timeSinceLast += Time.deltaTime;
            }
        }
    }
}
