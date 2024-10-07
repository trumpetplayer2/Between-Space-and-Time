using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using UnityEngine.Events;

namespace tp2
{
    public class Button : NetworkBehaviour
    {
        public bool isToggled;
        SpriteRenderer spriteRenderer;
        public float cooldown = 0.5f;
        public LayerMask m_LayerMask;
        public float sizex = 2;
        public float sizey = 2;
        [SerializeField] private UnityEvent<bool> ButtonUpdated;
        

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
            try
            {
                if (hitColliders == null)
                {
                    Debug.Log("ERROR: Negative button press");
                    toggleRpc(false);
                }
                else if (hitColliders.Length == 0)
                {
                    toggleRpc(false);
                }
                else if (hitColliders.Length > 0)
                {
                    toggleRpc(true);
                }
            }
            catch { }
        }

        [Rpc(SendTo.Everyone)]
        void toggleRpc(bool state)
        {
            isToggled = state;
            if (spriteRenderer != null)
            {
                //Modify the look of button.
                //This will be 2 sprites later on, but for now lets just recolor sprite renderer
                if (isToggled)
                {
                    spriteRenderer.color = Color.red;
                }
                else
                {
                    spriteRenderer.color = Color.white;
                }
            }
            ButtonUpdated.Invoke(isToggled);
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
