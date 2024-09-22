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
    int pressCount = 0;
    bool stateChanged = false;
    [SerializeField] private UnityEvent<bool> ButtonUpdated;

    private void Start()
    {
        if(spriteRenderer == null)
        {
            spriteRenderer = this.gameObject.GetComponent<SpriteRenderer>();
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!(collision.gameObject.tag.ToLower().Equals("player") || collision.gameObject.tag.ToLower().Equals("box"))) return;
        stateChanged = true;
        updatePressCountRpc(1);
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (!(collision.gameObject.tag.ToLower().Equals("player") || collision.gameObject.tag.ToLower().Equals("box"))) return;
        updatePressCountRpc(-1);
        stateChanged = true;
    }

    private void Update()
    {
        if (!stateChanged) return;
        if(pressCount < 0)
        {
            Debug.Log("ERROR: Negative button press");
        }else if(pressCount == 0)
        {
            toggleRpc(false);
        }else if(pressCount > 0)
        {
            toggleRpc(true);
        }
    }

    [Rpc(SendTo.Server)]
    void updatePressCountRpc(int i)
    {
        pressCount += i;
        syncPressCountRpc(pressCount);
    }

    [Rpc(SendTo.NotServer)]
    void syncPressCountRpc(int p)
    {
        pressCount = p;
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
