using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace tp2
{
    public class PointUnlocker : MonoBehaviour
    {
        public bool invert = false;
        public Patrol[] patrols;
        public void unlockToggle(bool value)
        {
            if (invert)
            {
                value = !value;
            }
            foreach(Patrol p in patrols)
            {
                if (value)
                {
                    p.addLocation(this.transform);
                }
                else
                {
                    p.removeLocation(this.transform);
                }
            }
        }
    }
}
