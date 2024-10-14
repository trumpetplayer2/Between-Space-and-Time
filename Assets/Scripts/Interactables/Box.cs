using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using UnityEngine.SceneManagement;

namespace tp2
{
    public class Box : NetworkBehaviour
    {
        Rigidbody2D body;
        bool held = false;
        public NetworkVariable<Vector3> Position = new NetworkVariable<Vector3>();
        bool curGrabbed = false;
        float cooldown = 0f;
        float weight;
        GameObject[] capableGrab = new GameObject[2];
        int layer = 0;
        private void Start()
        {
            body = GetComponent<Rigidbody2D>();
            weight = body.mass;
            layer = gameObject.layer;
            SceneManager.sceneUnloaded += OnSceneUnloaded;
        }
        private void OnTriggerEnter2D(Collider2D collision)
        {
            if (!(collision.gameObject.tag.ToLower().Equals("player"))) return;
            if(collision.gameObject.layer == 7)
            {
                capableGrab[1] = collision.gameObject;
            }else if(collision.gameObject.layer == 6)
            {
                capableGrab[0] = collision.gameObject;
            }
        }

        private void OnTriggerExit2D(Collider2D collision)
        {
            if (!(collision.gameObject.tag.ToLower().Equals("player"))) return;
            if (collision.gameObject.layer == 7)
            {
                capableGrab[1] = null;
            }
            else if (collision.gameObject.layer == 6)
            {
                capableGrab[0] = null;
            }

        }

        public void Update()
        {
            if (cooldown > 0)
            {
                cooldown -= Time.deltaTime;
                if (cooldown < 0)
                {
                    cooldown = 0;
                }
            }
            if (IsServer)
            {
                UpdateLocationRpc(this.transform.position);
            }
            if (IsOwner)
            {
                SubmitPositionRequestRpc(transform.position);
            }
            //Watch for input
            if (Input.GetButton("Interact"))
            {
                if (cooldown <= 0)
                {
                    if (curGrabbed)
                    {
                        //Make sure player who clicked R is the parent
                        if (NetPlayer.pressedR == null) return;
                        if(this.transform.parent == null) { release(); return; }
                        if (!this.gameObject.transform.parent.Equals(NetPlayer.pressedR.gameObject.transform)) return;
                        release();
                    }
                    else
                    {
                        //Prevent player from grabbing a box next to another player
                        if (NetPlayer.pressedR == null) return;
                        PlayerType temp = PlayerType.None;
                        if(NetPlayer.pressedR.gameObject.layer == 6)
                        {
                            if (capableGrab[0] == null) return;
                            temp = PlayerType.Atlas;
                        }else
                        if (NetPlayer.pressedR.gameObject.layer == 7)
                        {
                            if (capableGrab[1] == null) return;
                            temp = PlayerType.Chroma;
                        }
                        grab(NetPlayer.pressedR, temp);
                    }
                }
            }
        }

        public void grab(NetworkObject grabber, PlayerType type)
        {
            if (held) return;
            updateParentRpc(type);
            updateOwnerRpc(grabber.OwnerClientId);
            updateHolderRpc(true);
            curGrabbed = true;
            rigidBodyStuffRpc(true);
            cooldown = 0.5f;

        }
        public void release()
        {
            if (!held) return;
            updateParentRpc(PlayerType.None);
            updateHolderRpc(false);
            curGrabbed = false;
            rigidBodyStuffRpc(false);
        }

        [Rpc(SendTo.Server)]
        public void updateOwnerRpc(ulong owner)
        {
            this.NetworkObject.ChangeOwnership(owner);
        }

        [Rpc(SendTo.Server)]
        public void updateParentRpc(PlayerType type)
        {
            Transform newParent = null;
            switch (type)
            {
                case PlayerType.Atlas:
                    newParent = NetManager.instance.players[0].transform;
                    break;
                case PlayerType.Chroma:
                    newParent = NetManager.instance.players[1].transform;
                    break;
            }
            this.transform.parent = newParent;
            if (gameObject.transform.parent == null)
            {
                //updateLayerRpc(layer);
            }
            else
            {
                updateLayerRpc(this.transform.parent.gameObject.layer);
            }
        }
        [Rpc(SendTo.Everyone)]
        void updateLayerRpc(int newlayer)
        {
            gameObject.layer = newlayer;
            
        }

        [Rpc(SendTo.Everyone)]
        public void rigidBodyStuffRpc(bool destroy)
        {
            if (destroy)
            {
                Destroy(body);
                return;
            }
            else
            {
                body = this.gameObject.AddComponent<Rigidbody2D>();
                body.constraints = RigidbodyConstraints2D.FreezeRotation | RigidbodyConstraints2D.FreezePositionX;
                cooldown = 0.5f;
                body.mass = weight;
            }
        }

        //If it successfully updated, return true, if isHeld is already the same state, return false
        //This is to prevent potential double grab desync
        [Rpc(SendTo.Everyone)]
        public void updateHolderRpc(bool isHeld)
        {
            if (this.held == isHeld) return;
            this.held = isHeld;
        }

        [Rpc(SendTo.Server)]
        void SubmitPositionRequestRpc(Vector3 Pos, RpcParams rpcParams = default)
        {
            transform.position = Pos;
            Position.Value = Pos;
        }

        [Rpc(SendTo.NotOwner)]
        public void UpdateLocationRpc(Vector3 Pos, RpcParams rpcParams = default)
        {
            transform.position = Pos;
        }
        
        void OnSceneUnloaded(Scene current)
        {
            if (this != null)
            {
                Destroy(this.gameObject);
            }
        }
    }
}