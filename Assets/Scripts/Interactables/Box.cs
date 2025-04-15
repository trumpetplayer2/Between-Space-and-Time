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
        public NetworkVariable<int> objLayer = new NetworkVariable<int>();
        //bool curGrabbed = false;
        float cooldown = 0f;
        float weight;
        GameObject[] capableGrab = new GameObject[2];
        int layer = 0;
        public float heldWeight = 1f;
        public float fastThreshold = 0.5f;
        public float fastMult = 5;
        public Vector2 speedCap = new Vector2(6f, 6f);
        SpriteRenderer sprite;
        Vector3 alignPos = new Vector3(0,0,0);
        public float dropDistance = 1f;
        public bool isParadox = false;
        public float miny = -100;
        Transform startLocation;
        Rigidbody2D parentBody = null;
        public Color heldColor;
        Color baseColor;
        public float speedToSound = 3f;
        float lastPosy = 0;
        bool makeNoise = false;
        //TargetJoint2D joint = null;
        SfxHandler sfx;
        private void Start()
        {
            body = GetComponent<Rigidbody2D>();
            sfx = GetComponent<SfxHandler>();
            weight = body.mass;
            layer = gameObject.layer;
            SceneManager.sceneUnloaded += OnSceneUnloaded;
            if (IsServer)
            {
                objLayer.Value = this.gameObject.layer;
            }
            objLayer.OnValueChanged += updateLayer;
            GameObject temp = new GameObject("BoxStart");
            temp.transform.position = this.transform.position;
            startLocation = temp.transform;
            sprite = GetComponent<SpriteRenderer>();
            if(sprite != null)
            {
                baseColor = sprite.color;
            }
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

        private void FixedUpdate()
        {
            bodyUpdate();
            //targetUpdate();
            lastPosy = transform.position.y;
            if (!held.Value)
            {
                if(speedToSound < Mathf.Abs(lastPosy - transform.position.y))
                {
                    makeNoise = true;
                }
                if(lastPosy - transform.position.y < 0.01)
                {
                    if (makeNoise)
                    {
                        sfxRpc(1);
                        makeNoise = false;
                    }
                }
            }
        }

        public void grabAppearance(bool isHeld)
        {
            //Grab appearance
            if (sprite == null) return;
            if (isHeld)
            {
                sprite.color = heldColor;
            }
            else
            {
                sprite.color = baseColor;
            }
        }

        public void Update()
        {
            if (cooldown > 0)
            {
                cooldown -= Time.deltaTime;
                //Box shouldnt fly away
                if(Vector3.Distance(Vector3.zero, body.velocity) > 10f && !localHeld())
                {
                    body.velocity = Vector3.zero;
                }
                if (cooldown < 0)
                {
                    cooldown = 0;
                }
            }
            if(this.transform.position.y < miny)
            {
                this.transform.position = startLocation.position;
                if(body != null)
                {
                    body.velocity = Vector2.zero;
                }
                release("Reset Location");
            }
            //if (held.Value && transform.parent == null)
            //{
            //    switch (PlayerTypeExtensions.getFromBoxLayer(gameObject.layer))
            //    {
            //        case PlayerType.Atlas:
            //            if (PlayerTypeExtensions.AtlasObject == null) return;
            //            updateServerRpc(PlayerType.Atlas, true);
            //            return;
            //        case PlayerType.Chroma:
            //            if (PlayerTypeExtensions.ChromaObject == null) return;
            //            updateServerRpc(PlayerType.Chroma, true);
            //            return;
            //        default:
            //            release("Conflicting Parent and Held Bool - Held was True");
            //            return;
            //    }
            //}
            //if(!held.Value && transform.parent != null)
            //{
            //    PlayerType temp = PlayerTypeExtensions.getLocalPlayerType();
            //    PlayerType parentTemp = PlayerTypeExtensions.getEnumOf(transform.parent.gameObject);
            //    if(temp == parentTemp)
            //    {
            //        transform.parent = null;
            //        Debug.Log("Parent set to null");
            //    }
            //}
            //If held.value is true, and transform.parent == null or if held.value is false, and transform.parent exists, return. Network delay is occuring, and will cause problems
            if(!(held.Value ^ transform.parent == null))
            {
                if (transform.parent == null)
                {
                    release("ERROR: Held and Parent dont match");
                    return;
                }
                else
                {
                    if (transform.parent.tag.ToLower().Equals("player"))
                    {
                        release("ERROR: Held and Parent dont match");
                        return;
                    }
                }
            }
            //Distance check
            if (transform.parent != null)
            {
                //Only parent client can enforce distance drop
                if (transform.parent == PlayerTypeExtensions.getLocalPlayer().transform)
                {
                    if (Vector2.Distance(transform.parent.position, transform.position) > dropDistance)
                    {
                        release("Player too far");
                    }
                }
            }
            //Watch for input
            if (Input.GetButton("Interact"))
            {
                if (cooldown <= 0)
                {
                    if (localHeld())
                    {
                        //Make sure player who clicked R is the parent
                        if (NetPlayer.pressedR == null) return;
                        if(this.transform.parent == null) { release("Invalid Parent"); return; }
                        if (!this.gameObject.transform.parent.Equals(NetPlayer.pressedR.gameObject.transform)) return;
                        release("Drop Key Pressed");
                    }
                    else
                    {
                        //Prevent player from grabbing a box next to another player
                        if (NetPlayer.pressedR == null) return;
                        bool capable = false;
                        foreach(GameObject obj in capableGrab)
                        {
                            if (obj == null) continue;
                            if(obj == NetPlayer.pressedR.gameObject)
                            {
                                capable = true;
                            }
                        }
                        if (!capable) return;
                        grab(PlayerTypeExtensions.getTypeof(NetPlayer.pressedR.gameObject));
                    }
                }
            }
        }

        void bodyUpdate()
        {
            if (cooldown > 0 && !localHeld()) return;
            if (!IsOwner) return;
            if (body != null)
            {
                if (transform.parent != null)
                {
                    if (!transform.parent.tag.ToLower().Equals("player")) return;
                    if (alignPos == Vector3.zero && localHeld())
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

        bool localHeld()
        {
            if (this.transform.parent == null) return false;
            return (PlayerTypeExtensions.localPlayer.transform == this.transform.parent);
        }
        
        public void grab(PlayerType type)
        {
            if (held.Value) return;
            updateServerRpc(type, true);
            //curGrabbed = true;
            rigidBodyStuffRpc(true);
            cooldown = 0.5f;
            try
            {
                PlayerTypeExtensions.getObject(type).GetComponent<NetPlayer>().updateAnimationRpc(animationState.Box, true);
            }
            catch { }
            sfxRpc(0);
        }
        [Rpc(SendTo.Everyone)]
        public void sfxRpc(int clip)
        {
            sfx?.playClip(clip);
        }
        public void release(string reason)
        {
            if (!held.Value) return;
            try
            {
                this.gameObject.transform.parent.GetComponent<NetPlayer>().updateAnimationRpc(animationState.Box, false);
            }
            catch { }
            updateServerRpc(PlayerType.None, false);
            //curGrabbed = false;
            rigidBodyStuffRpc(false);
            alignPos = new Vector3(0, 0, 0);
            body.velocity = Vector3.zero;
        }

        [Rpc(SendTo.Server)]
        public void updateServerRpc(PlayerType type, bool isHeld)
        {
            ulong owner = PlayerTypeExtensions.getUserId(type);
            if (owner < 5)
            {
                updateOwner(owner);
            }
            updateParent(type);
            this.held.Value = isHeld;
        }

        void updateOwner(ulong owner)
        {
            this.NetworkObject.ChangeOwnership(owner);
        }

        void updateParent(PlayerType type)
        {
            Transform temp = null;
            if(PlayerTypeExtensions.getObject(type) != null)
            {
                temp = PlayerTypeExtensions.getObject(type).transform;
            }
            this.transform.parent = temp;
            if (!isParadox) return;
            if (gameObject.transform.parent != null)
            {
                objLayer.Value = (PlayerTypeExtensions.getBoxLayer(PlayerTypeExtensions.getEnumOf(this.transform.parent.gameObject.layer)));
            }
        }

        void updateLayer(int prev, int newlayer)
        {
            gameObject.layer = newlayer;
        }

        [Rpc(SendTo.Everyone)]
        public void rigidBodyStuffRpc(bool isHeld)
        {
            if (isHeld)
            {
                body.mass = heldWeight;
                //joint = gameObject.AddComponent<TargetJoint2D>();
                //joint.autoConfigureTarget = false;
                //joint.frequency = 100;
                if(transform.parent != null)
                {
                    parentBody = transform.parent.GetComponent<Rigidbody2D>();
                }
            }
            else
            {
                body.constraints = RigidbodyConstraints2D.FreezeRotation;
                cooldown = 0.5f;
                body.mass = weight;
                //Destroy(joint);
                //joint = null;
                parentBody = null;
            }
            grabAppearance(isHeld);
        }

        [Rpc(SendTo.Server)]
        void SubmitPositionRequestRpc(Vector3 Pos, RpcParams rpcParams = default)
        {
            transform.position = Pos;
            Position.Value = Pos;
        }

        [Rpc(SendTo.NotOwner)]
        void UpdateLocationRpc(Vector3 Pos, RpcParams rpcParams = default)
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