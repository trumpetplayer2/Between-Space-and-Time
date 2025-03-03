using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace tp2
{
    public class InvertActive : SfxHandler
    {
        public CameraShakeVar CameraShake = new CameraShakeVar(1f, 10f);
        private void Start()
        {
            if(CameraShake == null)
            {
                CameraShake = new CameraShakeVar(1f, 10f, PlayerTypeExtensions.getPlayerVisible(gameObject.layer));
            }
        }
        public void setInvertActive(bool t)
        {
            CameraFollow.instance.shake(CameraShake);
            playClip(!t, !t);
            this.gameObject.SetActive(!t);
        }
    }
}
