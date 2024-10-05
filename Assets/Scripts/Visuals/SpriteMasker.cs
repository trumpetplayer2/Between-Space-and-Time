using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace tp2
{
    public class SpriteMasker : MonoBehaviour
    {
        public PlayerType display;
        // Start is called before the first frame update
        void Start()
        {
            if(PlayerTypeExtensions.localPlayer == null)
            {
                //No Local Player, delay destroy
                Invoke("delayedCheck", 1);
                return;
            }
            //If Display is none, both players see it (Foreground object)
            if (!PlayerTypeExtensions.getLocalPlayerType().Equals(display) && display != PlayerType.None)
            {
                Destroy(this.gameObject);
            }
        }

        void delayedCheck()
        {
            if (PlayerTypeExtensions.localPlayer == null)
            {
                //No Local Player, delay destroy
                Invoke("delayedCheck", 1);
                return;
            }
            //If Display is none, both players see it (Foreground object)
            if (!PlayerTypeExtensions.getLocalPlayerType().Equals(display) && display != PlayerType.None)
            {
                Destroy(this.gameObject);
            }
        }
    }
}
