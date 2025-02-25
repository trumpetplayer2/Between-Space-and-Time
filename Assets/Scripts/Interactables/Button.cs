using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using UnityEngine.Events;

namespace tp2
{
    public class Button : NetworkBehaviour
    {
        public NetworkVariable<bool> isToggled = new NetworkVariable<bool>(false);
        public SpriteRenderer button;
        public float cooldown = 0.5f;
        public LayerMask m_LayerMask;
        public float sizex = 2;
        public float sizey = 2;
        public Sprite unpressed;
        public Sprite pressed;
        SfxHandler sfx;
        [SerializeField] private UnityEvent<bool> ButtonUpdated;
        bool localPressed = false;

        private void Start()
        {
            sfx = this.GetComponent<SfxHandler>();
            ButtonUpdated.AddListener((bool call) => playClip(call));
        }

        void playClip(bool t = false)
        {
            sfx?.playClip(0);
        }

        private void Update()
        {
            Collider2D[] hitColliders = Physics2D.OverlapBoxAll(this.transform.position, new Vector2(sizex, sizey), 0f, m_LayerMask);
            bool temp = isToggled.Value;
            try
            {
                if (hitColliders == null)
                {
                    Debug.Log("ERROR: Negative button press");
                    temp = false;
                }
                else if (hitColliders.Length == 0)
                {
                    temp = false;
                }
                else if (hitColliders.Length > 0)
                {
                    temp = true;
                }
                if (temp != isToggled.Value)
                {
                    if (IsServer)
                    {
                        isToggled.Value = temp;
                    }
                }
                if(localPressed != isToggled.Value)
                {
                    localPressed = isToggled.Value;
                    ButtonUpdated.Invoke(isToggled.Value);
                }
            }
            catch { }
            if (button != null)
            {
                //Modify the look of button.
                //This will be 2 sprites later on, but for now lets just recolor sprite renderer
                if (isToggled.Value)
                {
                    button.sprite = pressed;
                }
                else
                {
                    button.sprite = unpressed;
                }
            }
        }

        public void updateAtlasFinish(bool state)
        {
            NetManager.instance.updateAtlasFinish(state);
        }
        public void updateChromaFinish(bool state)
        {
            NetManager.instance.updateChromaFinish(state);
        }
    }
}
