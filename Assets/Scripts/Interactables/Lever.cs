using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using UnityEngine.Events;

public class Lever : NetworkBehaviour
{
    public bool isToggled;
    SpriteRenderer spriteRenderer;
    public bool onWall = false;
    public float cooldown = 0.5f;
    float curCooldown = 0;
    bool canToggle = false;
    [SerializeField] private UnityEvent<bool> LeverFlipped;

    private void Start()
    {
        if(spriteRenderer == null)
        {
            spriteRenderer = this.gameObject.GetComponent<SpriteRenderer>();
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!collision.gameObject.tag.ToLower().Equals("player")) return;
        canToggle = true;
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (!collision.gameObject.tag.ToLower().Equals("player")) return;
        canToggle = false;
    }

    public void Update()
    {
        if(curCooldown > 0)
        {
            curCooldown -= Time.deltaTime;
            if(curCooldown < 0)
            {
                curCooldown = 0;
            }
        }
        if (Input.GetButton("Interact") && curCooldown <= 0 && canToggle)
        {
            toggleRpc();
        }
    }

    [Rpc(SendTo.Everyone)]
    void toggleRpc()
    {
        isToggled = !isToggled;
        if(spriteRenderer != null)
        {
            if (onWall)
            {
                spriteRenderer.flipY = isToggled;
            }
            else
            {
                spriteRenderer.flipX = isToggled;
            }
        }
        curCooldown = cooldown;
        LeverFlipped.Invoke(isToggled);
    }
}
