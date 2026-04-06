using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class monstermove : MonoBehaviour
{
    Rigidbody2D rigid;
    public int nextMove;
    Animator anim;
    SpriteRenderer spriteRenderer;
    CapsuleCollider2D capsuleCollider;
    void Awake()
    {
        rigid = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        capsuleCollider = GetComponent<CapsuleCollider2D>();

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
            // [주의] 몬스터 몸통 중심(rigid.position.x)에서 떨어진 거리(0.3f)가 몬스터의 실제 Collider 크기와 안 맞으면 
            // 허공에서 멈추거나 절벽에서 너무 늦게 도는 문제가 생길 수 있습니다. Collider bounds 사이즈를 기반으로 하는 것이 안전합니다.
            Vector2 frontVec = new Vector2(rigid.position.x + nextMove * 0.3f, rigid.position.y);
            Debug.DrawRay(frontVec, Vector3.down, new Color(0, 1, 0));
            RaycastHit2D rayHit = Physics2D.Raycast(frontVec, Vector3.down, 1, LayerMask.GetMask("Platform"));

            if (rayHit.collider == null)
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
        if (nextMove != 0)
        {
            spriteRenderer.flipX = nextMove == 1;
        }

        //Recursive
        float nextThinkTime = Random.Range(2f, 5f);
        Invoke("Think", nextThinkTime);
    }

    void Turn()
    {
        nextMove *= -1;
        spriteRenderer.flipX = nextMove == 1;

        CancelInvoke("Think");
        Invoke("Think", 5);

    }

    public void OnDamaged()
    {
        // [주의] 이미 사망하여 통통 튀고 있는 몬스터를 연속으로 공격(밟기)하면 OnDamaged가 여러 번 불려 다시 위로 튀어오를 수 있습니다.
        // 불리언 변수(isDead)를 추가하여 한 번만 실행되도록 보호하는 것이 좋습니다.

        //sprite Alpha
        spriteRenderer.color = new Color(1, 1, 1, 0.4f);
        //Sprite Flip
        spriteRenderer.flipY = true;
        //Collider Disable
        capsuleCollider.enabled = false;
        //Die Effect Jump
        rigid.AddForce(Vector2.up * 5, ForceMode2D.Impulse);
        //Destroy
        Invoke("DeActive", 5);

    }

    void DeActive()
    {
        gameObject.SetActive(false);
    }
}
