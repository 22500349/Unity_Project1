using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class PlayerMove : MonoBehaviour 
{
    public float maxSpeed;
    Rigidbody2D rigid;

    void Awake()
    {
        rigid = GetComponent<Rigidbody2D>();
    }

    void Update()
    {   
        //Stop Speed
        if(Input.GetButtonUp("Horizontal")){
            rigid.linearVelocity = new Vector2(rigid.linearVelocity.normalized.x * 0.5f, rigid.linearVelocity.y);
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
    }
}