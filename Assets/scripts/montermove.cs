using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class montermove : MonoBehaviour
{
    Rigidbody2D rigid;
    public int nextMove;
    Animator anim;
    SpriteRenderer spriteRenderer;
    void Awake()
    {
        rigid = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();

        nextMove = 0;

        Invoke("Think", 3);
    }

    // Update is called once per frame
    void FixedUpdate()
    {   
        // Move
        rigid.linearVelocity = new Vector2(nextMove, rigid.linearVelocity.y);

        // 🌟 1. 몬스터가 현재 바닥에 닿아있는지 '발밑'을 먼저 검사합니다.
        Vector2 centerVec = new Vector2(rigid.position.x, rigid.position.y);
        RaycastHit2D groundHit = Physics2D.Raycast(centerVec, Vector3.down, 1, LayerMask.GetMask("Platform"));

        // 🌟 2. 발밑에 바닥이 있을 때만! (즉, 공중에 떨어지는 중이 아닐 때만) 절벽 검사를 합니다.
        if (groundHit.collider != null && nextMove != 0) 
        {
            // Platform Check (기존에 작성하신 앞쪽 절벽 검사 레이저)
            Vector2 frontVec = new Vector2(rigid.position.x + nextMove * 0.3f, rigid.position.y);
            Debug.DrawRay(frontVec, Vector3.down, new Color(0, 1, 0));
            RaycastHit2D rayHit = Physics2D.Raycast(frontVec, Vector3.down, 1, LayerMask.GetMask("Platform"));

            if(rayHit.collider == null)
            {
                Turn();
            }
        } 
    }


    //재귀 함수
    void Think()
    {   
        //Set next Active
        nextMove = Random.Range(-1, 2);

        //Sprite Animation
        anim.SetInteger("WalkSpeed", nextMove);

        //Flip Sprite
        if(nextMove != 0) {
            spriteRenderer.flipX = nextMove == 1;
        }

        //Recursive
        float nextThinkTime = Random.Range(2f,5f);
        Invoke("Think", nextThinkTime);
    }

    void Turn()
    {
        nextMove *= -1;
        spriteRenderer.flipX = nextMove == 1;

        CancelInvoke("Think");
        Invoke("Think", 5);
        
    }
}
