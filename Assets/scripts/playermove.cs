using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class PlayerMove : MonoBehaviour 
{
    public float maxSpeed;
    public float jumpPower;
    Rigidbody2D rigid;
    SpriteRenderer spriteRenderer;
    Animator anim;

    void Awake()
    {
        rigid = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        anim = GetComponent<Animator>();
    }

    void Update()
    {  
        //Jump
        if(Input.GetButtonDown("Jump") && !anim.GetBool("isJumping")){
            rigid.AddForce(Vector2.up * jumpPower, ForceMode2D.Impulse);
            anim.SetBool("isJumping", true);

        }

        //Stop Speed
        if(Input.GetButtonUp("Horizontal")){
            rigid.linearVelocity = new Vector2(rigid.linearVelocity.normalized.x * 0.5f, rigid.linearVelocity.y);
        }

        //Direction Sprite
        if(Input.GetButtonDown("Horizontal")){
            spriteRenderer.flipX = Input.GetAxisRaw("Horizontal") == -1;
        }

        //Walk Animation
        if(Mathf.Abs( rigid.linearVelocity.x ) < 1){
            anim.SetBool("isWalking", false);
        }
        else{
           anim.SetBool("isWalking", true);
        }
    }

    void FixedUpdate()
    {
        //Move Speed And Key Control 
        float h = Input.GetAxisRaw("Horizontal");
        rigid.AddForce(Vector2.right * h, ForceMode2D.Impulse);

        if(rigid.linearVelocity.x > maxSpeed){ //Right max speed
            rigid.linearVelocity = new Vector2(maxSpeed, rigid.linearVelocity.y);
        }

        else if(rigid.linearVelocity.x < maxSpeed*(-1)){  //Left max speed
            rigid.linearVelocity = new Vector2(maxSpeed*(-1), rigid.linearVelocity.y);
        }

        //Landing Platform
        if(rigid.linearVelocity.y < 0){
            Debug.DrawRay(rigid.position, Vector3.down, new Color(0,1,0));
            RaycastHit2D rayHit = Physics2D.Raycast(rigid.position, Vector3.down, 1, LayerMask.GetMask("Platform"));

            if(rayHit.collider != null){
                if(rayHit.distance < 0.5f){
                    anim.SetBool("isJumping", false);
                    Debug.Log(rayHit.collider.name);
                }
            }
        }

    }
}