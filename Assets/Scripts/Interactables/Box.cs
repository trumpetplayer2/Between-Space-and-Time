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
        NetworkVariable<bool> held = new NetworkVariable<bool>(false);
        public NetworkVariable<Vector3> Position = new NetworkVariable<Vector3>();
        bool curGrabbed = false;
        float cooldown = 0f;
        float weight;
        GameObject[] capableGrab = new GameObject[2];
        int layer = 0;
        public float heldWeight = 1f;
        public float fastThreshold = 0.5f;
        public float fastMult = 3;
        Vector3 alignPos = new Vector3(0,0,0);
        public float dropDistance = 1f;
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
                //Box shouldnt fly away
                if(Vector3.Distance(Vector3.zero, body.velocity) > 10f && !curGrabbed)
                {
                    body.velocity = Vector3.zero;
                }
                if (cooldown < 0)
                {
                    cooldown = 0;
                }
            }
            bodyUpdate();
            //Distance check
            if (transform.parent != null)
            {
                if (Vector2.Distance(transform.parent.position, transform.position) > dropDistance)
                {
                    release();
                }
            }
            //if (IsServer)
            //{
            //    UpdateLocationRpc(this.transform.position);
            //}
            //if (IsOwner)
            //{
            //    SubmitPositionRequestRpc(transform.position);
            //}
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

        void bodyUpdate()
        {
            if (cooldown > 0 && !curGrabbed) return;
            if (!IsOwner) return;
            if (body != null)
            {
                if (transform.parent != null)
                {
                    if (alignPos == Vector3.zero && curGrabbed)
                    {
                        float sign = Mathf.Sign(transform.parent.InverseTransformPoint(transform.position).x);
                        if (sign == float.NaN) sign = 0;
                        Vector3 bSize = transform.localScale;
                        alignPos = new Vector3((bSize.x / 2 + .6f) * sign, 0.01f, transform.position.z);
                    }
                    else
                    {
                        //Apply a velocity to try to get to goal location. If object is in the way this wont move
                        body.velocity = (alignPos - transform.parent.InverseTransformPoint(transform.position)) / 0.1f;
                        if (Mathf.Abs(body.velocity.x) + Mathf.Abs(body.velocity.y) > fastThreshold)
                        {
                            body.velocity = body.velocity * fastMult;
                        }
                    }
                }
                else
                {
                    //Prevent box from flying at extreme speeds
                    if (Mathf.Abs(body.velocity.x) + Mathf.Abs(body.velocity.y) > 10)
                    {
                        if (body.velocity.x > 0.1)
                        {
                            body.velocity = Vector3.zero;
                        }
                        else
                        {
                            float temp = Mathf.Abs(body.velocity.x) + Mathf.Abs(body.velocity.y);
                            body.velocity = new Vector3(Mathf.Min(body.velocity.x/temp * 10, 5), Mathf.Min(body.velocity.y/temp * 10, 10));
                        }
                    }
                }
            }
            if (IsHost)
            {
                UpdateLocationRpc(transform.position);
            }
            else
            {
                SubmitPositionRequestRpc(transform.position);
            }
        }

        public void grab(NetworkObject grabber, PlayerType type)
        {
            if (held.Value) return;
            updateParentRpc(type);
            updateOwnerRpc(grabber.OwnerClientId);
            updateHolderRpc(true);
            curGrabbed = true;
            rigidBodyStuffRpc(true);
            cooldown = 0.5f;
        }
        public void release()
        {
            if (!held.Value) return;
            updateParentRpc(PlayerType.None);
            updateHolderRpc(false);
            curGrabbed = false;
            rigidBodyStuffRpc(false);
            alignPos = new Vector3(0, 0, 0);
            body.velocity = Vector3.zero;
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
                updateLayerRpc(PlayerTypeExtensions.getBoxLayer(PlayerTypeExtensions.getEnumOf(this.transform.parent.gameObject.layer)));
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
                body.mass = heldWeight;
            }
            else
            {
                body.constraints = RigidbodyConstraints2D.FreezeRotation;
                cooldown = 0.5f;
                body.mass = weight;
            }
        }

        //If it successfully updated, return true, if isHeld is already the same state, return false
        //This is to prevent potential double grab desync
        [Rpc(SendTo.Server)]
        public void updateHolderRpc(bool isHeld)
        {
            if (this.held.Value == isHeld) return;
            this.held.Value = isHeld;
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