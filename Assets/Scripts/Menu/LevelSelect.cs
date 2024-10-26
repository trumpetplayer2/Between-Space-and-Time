using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace tp2
{
    public class LevelSelect : MonoBehaviour
    {
        public GameObject text;
        // Start is called before the first frame update
        void Start()
        {
            if (NetManager.instance == null) Destroy(this);
            NetManager.instance.menu = this.gameObject;
            hide();
        }

        void hide()
        {
            if (text != null)
            {
                text.SetActive(false);
            }
            this.gameObject.SetActive(false);
        }

        public void selectLevel(int level)
        {
            NetManager.instance.updateScene(level);
        }

        public void showClient()
        {
            disableButtons();
            if(text != null)
            {
                text.SetActive(true);
            }
        }

        void disableButtons()
        {
            UnityEngine.UI.Button[] buttons = this.GetComponentsInChildren<UnityEngine.UI.Button>();
            if (buttons == null) return;
            foreach(UnityEngine.UI.Button b in buttons)
            {
                b.interactable = false;
            }
        }
    }
}
