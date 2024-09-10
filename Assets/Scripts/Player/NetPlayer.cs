using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

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
    
    //Initalize when loaded
    public override void OnNetworkSpawn()
    {
        //Attempt to fetch sprite renderer if possible. This will be changed once there is a difference between player sprites
        SpriteRenderer temp = null;
        try
        {
            temp = this.gameObject.GetComponent<SpriteRenderer>();
        }
        catch { }
        //If the player is the owner, give it a rigid body and set up camera follow.
        if (IsOwner)
        {
            if (temp != null)
            {
                temp.color = Color.blue;
            }
            Player = gameObject.AddComponent<Rigidbody2D>();
            Player.freezeRotation = true;
            Player.gravityScale = gravityScale;
            CameraFollow.instance.playerTracker = this.tracker.transform;
            this.gameObject.layer = 7;
        }
        else
        {
            //If the player is not the owner, change color to signify so. This will be removed later
            temp.color = Color.red;
        }
    }

    [Rpc(SendTo.Server)]
    void SubmitPositionRequestRpc(Vector3 Pos, RpcParams rpcParams = default)
    {
        transform.position = Pos;
        Position.Value = Pos;
    }

    static Vector3 GetRandomPositionOnPlane()
    {
        return new Vector3(Random.Range(-3f, 3f), 1f, Random.Range(-3f, 3f));
    }

    private void FixedUpdate()
    {
        if (!IsOwner)
        {
            return;
        }
        if (Player == null) return;
        bool isOnGround = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);

        Vector2 move = new Vector2((Input.GetAxis("Horizontal") * speed), Player.velocity.y);
        Player.velocity = move;
        SubmitPositionRequestRpc(transform.position);
    }
    void Update()
    {
        if (IsServer)
        {
            UpdateLocationRpc(Position.Value);
        }
        //Player movement
        if (!IsOwner)
        {
            return;
        }
        if (Player == null) return;
        if (Input.GetButtonDown("Jump") && jumpTime < 0.1)
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
        bool isOnGround = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);
        if (isOnGround && !jumping)
        {
            jumpTime = 0;
        }
    }

    [Rpc(SendTo.NotOwner)]
    public void UpdateLocationRpc(Vector3 Pos, RpcParams rpcParams = default)
    {
        transform.position = Pos;
        Position.Value = Pos;
    }
}