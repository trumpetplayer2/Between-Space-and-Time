using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using UnityEngine.Events;

public class Button : NetworkBehaviour
{
    public bool isToggled;
    SpriteRenderer spriteRenderer;
    public bool onWall = false;
    public float cooldown = 0.5f;
    public LayerMask m_LayerMask;
    public float sizex = 2;
    public float sizey = 2;
    [SerializeField] private UnityEvent<bool> ButtonUpdated;

    private void Start()
    {
        if(spriteRenderer == null)
        {
            spriteRenderer = this.gameObject.GetComponent<SpriteRenderer>();
        }
    }

    private void Update()
    {
        Collider2D[] hitColliders = Physics2D.OverlapBoxAll(this.transform.position, new Vector2(sizex, sizey), 0f, m_LayerMask);
        if(hitColliders == null)
        {
            Debug.Log("ERROR: Negative button press");
            toggleRpc(false);
        }
        else if(hitColliders.Length == 0)
        {
            toggleRpc(false);
        }else if(hitColliders.Length > 0)
        {
            toggleRpc(true);
        }
    }

    [Rpc(SendTo.Everyone)]
    void toggleRpc(bool state)
    {
        isToggled = state;
        if(spriteRenderer != null)
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
}
