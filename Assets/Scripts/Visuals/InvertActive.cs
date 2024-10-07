using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace tp2
{
    public class InvertActive : MonoBehaviour
    {
        public void setInvertActive(bool t)
        {
            this.gameObject.SetActive(!t);
        }
    }
}
