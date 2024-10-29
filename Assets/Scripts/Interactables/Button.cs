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
        SpriteRenderer spriteRenderer;
        public float cooldown = 0.5f;
        public LayerMask m_LayerMask;
        public float sizex = 2;
        public float sizey = 2;
        [SerializeField] private UnityEvent<bool> ButtonUpdated;
        bool localPressed = false;
        

        private void Awake()
        {
            if (spriteRenderer == null)
            {
                spriteRenderer = this.gameObject.GetComponent<SpriteRenderer>();
            }
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
            if (spriteRenderer != null)
            {
                //Modify the look of button.
                //This will be 2 sprites later on, but for now lets just recolor sprite renderer
                if (isToggled.Value)
                {
                    spriteRenderer.color = Color.red;
                }
                else
                {
                    spriteRenderer.color = Color.white;
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
