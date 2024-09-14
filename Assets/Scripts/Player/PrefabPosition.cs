using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace tp2
{
    public class PrefabPosition : MonoBehaviour
    {
        public PlayerType positionPlayer;
        // Start is called before the first frame update
        void Start()
        {
            //Remove any dev indicators via deleting sprite renderer
            SpriteRenderer temp = GetComponent<SpriteRenderer>();
            if(temp != null)
            {
                Destroy(temp);
            }
            switch (positionPlayer)
            {
                case PlayerType.Atlas:
                    NetManager.aStart = this.transform;
                    break;
                case PlayerType.Chroma:
                    NetManager.cStart = this.transform;
                    break;
            }
        }
    }
}
