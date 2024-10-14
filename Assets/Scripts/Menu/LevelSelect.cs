using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace tp2
{
    public class LevelSelect : MonoBehaviour
    {
        // Start is called before the first frame update
        void Start()
        {
            if (NetManager.instance == null) Destroy(this);
            NetManager.instance.menu = this.gameObject;
            hide();
        }

        void hide()
        {
            this.gameObject.SetActive(false);
        }

        public void selectLevel(int level)
        {
            NetManager.instance.updateScene(level);
        }
    }
}
