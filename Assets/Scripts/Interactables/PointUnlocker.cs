using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace tp2
{
    public class PointUnlocker : MonoBehaviour
    {
        public Patrol[] patrols;
        
        public void unlockToggle(bool value)
        {
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
