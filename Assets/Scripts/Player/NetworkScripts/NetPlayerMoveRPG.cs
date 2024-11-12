using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using FishNet.Connection;
using FishNet.Object;


[RequireComponent(typeof(PlayerControls),typeof(Rigidbody2D))]
public class NetPlayerMoveRPG : NetworkBehaviour
{

    public static NetPlayerMoveRPG LocalPlayer { get; private set; }

    PlayerControls controls;
    Vector2 move;
    Rigidbody2D rb;
    Animator anim;
    public int speed;
    bool freeze = false;

    public override void OnStartClient()
    {
        base.OnStartClient();
        if (IsOwner)
        {
            //Start
            LocalPlayer = this;
            GameEvents.instance.OnLocalPlayerSpawn.Invoke();
        }
        else
        {

            GetComponent<Rigidbody2D>().isKinematic = true;
            enabled = false;
        }
    }
    void Start()
    {
        controls = GetComponent<PlayerControls>();
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
    }
    void Update()
    {
        if (!freeze)
        {
            Inputs();

            if (Input.GetKeyDown(controls.attack))
            {
                Freeze();
                anim.SetBool("Attack",true);
            }
        }
    }

    public void Freeze()
    {
        freeze = true;
        move = Vector2.zero;
    }
    public void UnFreeze()
    {
        anim.SetBool("Attack", false);
        freeze = false;
    }


    private void FixedUpdate()
    {
        if (move != Vector2.zero) { 
            rb.MovePosition((Vector2)transform.position + (move.normalized * speed * Time.fixedDeltaTime));
            anim.SetFloat("X", move.x);
            anim.SetFloat("Y", move.y);
            anim.SetBool("Walking", true);
        }
        else
        {
            anim.SetBool("Walking", false);

        }
    }


    private void Inputs()
    {
        if (Input.GetKey(controls.right))
        {
            move.x = 1;
        }
        else if (Input.GetKey(controls.left))
        {
            move.x = -1;
        }
        else
        {
            move.x = 0;
        }

        if (Input.GetKey(controls.up))
        {
            move.y = 1;
        }
        else if (Input.GetKey(controls.down))
        {
            move.y = -1;
        }
        else
        {
            move.y = 0;
        }
    }

}
