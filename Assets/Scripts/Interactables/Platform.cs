using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace tp2
{
    public class Platform : MonoBehaviour
    {
        public LayerMask mask;
        private void OnTriggerStay2D(Collider2D collision)
        {
            if((mask.value & (1 << collision.gameObject.layer)) == 0) { return; }
            
        }

        private void Update()
        {
            
        }
    }
}
