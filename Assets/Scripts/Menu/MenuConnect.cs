using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace tp2
{
    public class MenuConnect : MonoBehaviour
    {
        public void startHost()
        {
            NetManager.instance.startHost();
        }

        public void manualConnect(TMPro.TMP_InputField input)
        {
            NetManager.instance.manualConnect(input);
        }
    }
}
