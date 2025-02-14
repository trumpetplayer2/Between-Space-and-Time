using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace tp2
{
    public class JoinCode : MonoBehaviour
    {
        public TextMeshProUGUI text;

        // Update is called once per frame
        void Update()
        {
            if(text != null)
            {
                text.text = "Join Code: " + NetManager.instance.joinCode;
            }
        }
    }
}
