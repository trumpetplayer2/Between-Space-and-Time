using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace tp2
{
    public class InvertActive : MonoBehaviour
    {
        public float screenShake = 1f;
        public float shakeDuration = 1f;
        public void setInvertActive(bool t)
        {
            this.gameObject.SetActive(!t);
            CameraFollow.instance.shake(screenShake, shakeDuration);
        }
    }
}
