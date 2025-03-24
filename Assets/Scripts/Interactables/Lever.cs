using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using UnityEngine.Events;
namespace tp2 {
    public class Lever : NetworkBehaviour
    {
        public bool isToggled;
        public SpriteRenderer lever;
        public bool onWall = false;
        public float cooldown = 0.5f;
        float curCooldown = 0;
        float pullRadius = .5f;
        SfxHandler sfx;
        public LayerMask playerLayers;
        bool broken = false;
        [SerializeField] private UnityEvent<bool> LeverFlipped;

        private void Start()
        {
            sfx = this.GetComponent<SfxHandler>();
            LeverFlipped.AddListener((bool call) => playClip(call));
        }
        void playClip(bool t = false)
        {
            sfx?.playClip(0);
        }

        public void Update()
        {
            if (broken) return;
            if (curCooldown > 0)
            {
                curCooldown -= Time.deltaTime;
                if (curCooldown < 0)
                {
                    curCooldown = 0;
                }
            }
            //TODO: Switch to an Area detection rather than trigger that fetches the player and makes sure the player is "Pressed R" since that check is not here
            if (Input.GetButton("Interact") && curCooldown <= 0)
            {
                Collider2D[] puller = null;
                puller = Physics2D.OverlapCircleAll(transform.position, pullRadius, playerLayers);
                if (puller != null)
                {
                    bool playerConfirm = false;
                    foreach (Collider2D collider in puller)
                    {
                        if (NetPlayer.pressedR == null) break;
                        if (collider.gameObject == NetPlayer.pressedR.gameObject)
                        {
                            playerConfirm = true;
                            break;
                        }
                    }
                    if (playerConfirm)
                    {
                        toggleRpc();
                    }
                }
            }
        }

        public void toggleBroken(bool broke)
        {
            broken = broke;
            //TODO: Play breaking Audio Cue if true
            sfx?.playClip(1);
        }

        [Rpc(SendTo.Everyone)]
        void toggleRpc()
        {
            isToggled = !isToggled;
            if (lever != null)
            {
                if (onWall)
                {
                    lever.flipX = isToggled;
                }
                else
                {
                    lever.flipX = isToggled;
                }
            }
            curCooldown = cooldown;
            LeverFlipped.Invoke(isToggled);
        }
    }
}