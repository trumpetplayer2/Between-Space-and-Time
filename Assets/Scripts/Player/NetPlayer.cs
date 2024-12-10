using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

namespace tp2
{
    public class NetPlayer : NetworkBehaviour
    {
        //Create variables we will need later
        public NetworkVariable<Vector3> Position = new NetworkVariable<Vector3>();
        public GameObject tracker;
        Rigidbody2D Player = null;
        public float speed = 10;
        public float jumpforce = 10;
        public Transform groundCheck;
        public float groundCheckRadius;
        public LayerMask groundLayer;
        public float gravityScale = 2f;
        bool jumping = false;
        float jumpTime = 0f;
        public float maxJumpTime = 0.15f;
        public LayerMask AtlasMask;
        public LayerMask ChromaMask;
        public static NetworkObject pressedR;
        public static bool paused = false;
        public float maxVelocity = 20;
        public Vector2 maxAccel = new Vector2(20, 20);
        Vector2 priorVel = Vector3.zero;
        bool m_paused = false;
        public float minimumY = -100;
        float lastGroundedY = 0;
        //Initalize when loaded
        //public override void OnNetworkSpawn()
        //{
        //    //Attempt to fetch sprite renderer if possible. This will be changed once there is a difference between player sprites
        //    SpriteRenderer temp = null;
        //    try
        //    {
        //        temp = this.gameObject.GetComponent<SpriteRenderer>();
        //    }
        //    catch { }
        //    //If the player is the owner, give it a rigid body and set up camera follow.
        //    if (IsOwner)
        //    {
        //        initializeRpc();
        //    }
        //    DontDestroyOnLoad(this.gameObject);
        //}

        //TODO: SceneEventType.LoadComplete

        public void Start()
        {
            initializeRpc();
            //NetManager.networkUpdate.AddListener(NetworkUpdate);
        }

        [Rpc(SendTo.Owner)]
        public void updateCameraTrackerRpc(Vector3 pos)
        {
            CameraFollow.instance.playerTracker = this.tracker.transform;
            this.transform.position = pos;
            if (Player != null)
            {
                Player.velocity = new Vector3(0, 0, 0);
            }
            SubmitPositionRequestRpc(pos);
            delayCameraUpdate();
            NetManager.instance.endLoading();
        }

        public void delayCameraUpdate()
        {
            Invoke("updateCameraLayerMaskRpc", 1);
        }

        [Rpc(SendTo.Owner)]
        private void updateCameraLayerMaskRpc()
        {
            Camera c = Camera.main;
            PlayerType t = gameObject.GetComponent<InitializePlayer>().playerType;
            switch (t)
            {
                case PlayerType.Atlas:
                    c.gameObject.layer = this.gameObject.layer;
                    c.cullingMask = AtlasMask;
                    break;
                case PlayerType.Chroma:
                    c.gameObject.layer = this.gameObject.layer;
                    c.cullingMask = ChromaMask;
                    break;
            }
        }

        [Rpc(SendTo.Owner)]
        public void initializeRpc()
        {
            if (!IsClient) return;
            Player = gameObject.AddComponent<Rigidbody2D>();
            if (Player == null)
            {
                Player = gameObject.GetComponent<Rigidbody2D>();
            }
            Player.freezeRotation = true;
            Player.gravityScale = gravityScale;
            CameraFollow.instance.playerTracker = this.tracker.transform;
            if (IsOwner)
            {
                Camera c = Camera.main;
                PlayerType t = gameObject.GetComponent<InitializePlayer>().playerType;
                switch (t)
                {
                    case PlayerType.Atlas:
                        c.cullingMask = AtlasMask;
                        break;
                    case PlayerType.Chroma:
                        c.cullingMask = ChromaMask;
                        break;
                }
            }
        }


        [Rpc(SendTo.Server)]
        void SubmitPositionRequestRpc(Vector3 Pos, RpcParams rpcParams = default)
        {
            //Only update pos once per frame
            transform.position = Pos;
            Position.Value = Pos;
            UpdateLocationRpc(Pos);
        }

        private void FixedUpdate()
        {
            
            if (!IsClient) return;
            if (!IsOwner)
            {
                return;
            }
            if (Player == null) return;
            if(CameraFollow.instance.playerTracker == null)
            {
                //Client doesnt correctly connect camera tracker on first frame. Idk the proper way to fix but this should force it to double check
                CameraFollow.instance.playerTracker = this.transform;
            }
            if(m_paused != paused)
            {
                pauseChanged();
            }
            if (paused)
            {
                SubmitPositionRequestRpc(transform.position);
                return;
            }

            Vector2 move = new Vector2((Input.GetAxis("Horizontal") * speed), Player.velocity.y);

            Vector2 accel = new Vector2(move.x - priorVel.x, move.y - priorVel.x);
            //Cap Acceleration. This prevents random geometry flinging
            if(maxAccel.x < Mathf.Abs(accel.x) && !(move.x == 0))
            {
                move.x = priorVel.x;
            }
            if (maxAccel.y < Mathf.Abs(accel.y) && !(move.y == 0))
            {
                move.y = priorVel.y;
            }

            Player.velocity = move;
            SubmitPositionRequestRpc(transform.position);
            priorVel = Player.velocity;
        }


        void Update()
        {
            //Player movement
            if (!IsOwner)
            {
                return;
            }
            if (paused) return;
            if (Input.GetButton("Interact"))
            {
                pressedR = this.NetworkObject;
            }
            else
            {
                pressedR = null;
            }
            if (Player == null) return;
            bool isOnGround = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);
            if (isOnGround)
            {
                lastGroundedY = transform.position.y + 1;
            }
            if (Input.GetButtonDown("Jump") && jumpTime < 0.1 && isOnGround)
            {
                jumping = true;
            }
            if (Input.GetButton("Jump") && jumping)
            {
                if (jumpTime < maxJumpTime)
                {
                    float jumpHeight = (jumpforce - (jumpTime * 6)) * Time.deltaTime * 100;
                    if (jumpHeight < 0) jumpHeight = 0;
                    Player.AddForce(new Vector2(0, jumpHeight));
                }
                jumpTime += Time.deltaTime;
            }
            if (Input.GetButtonUp("Jump"))
            {
                jumping = false;
            }
            if (isOnGround && !jumping)
            {
                jumpTime = 0;
            }
            //Cap Velocity
            if(Mathf.Abs(Player.velocity.x) > maxVelocity || Mathf.Abs(Player.velocity.y) > maxVelocity)
            {
                //Cap Abs(X) to max velocity and Abs(Y) to max velocity. Make sure sign matches
                Vector2 sign = new Vector2(Mathf.Sign(Player.velocity.x), Mathf.Sign(Player.velocity.y));
                if(sign.x == float.NaN)
                {
                    sign.x = 0;
                }
                if(sign.y == float.NaN)
                {
                    sign.y = 0;
                }
                Vector2 newVelocity = new Vector2(Mathf.Min(Mathf.Abs(Player.velocity.x), maxVelocity)
                    * sign.x,
                    Mathf.Min(Mathf.Abs(Player.velocity.y), maxVelocity)
                    * sign.y); ;
                Player.velocity = newVelocity;
            }
            if(transform.position.y < minimumY)
            {
                transform.position = new Vector3(transform.position.x, lastGroundedY, transform.position.z);
                Player.velocity = Vector3.zero;
            }
        }

        public void pauseChanged()
        {
            if(Player != null)
            {
                Player.velocity = Vector3.zero;
            }
            m_paused = paused;
        }

        [Rpc(SendTo.NotOwner)]
        public void UpdateLocationRpc(Vector3 Pos, RpcParams rpcParams = default)
        {
            transform.position = Pos;
        }
    }
}