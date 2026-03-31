using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class PlayerMove : MonoBehaviour
{
    public GameManager gameManager;
    public float maxSpeed;
    public float jumpPower;
    Rigidbody2D rigid;
    SpriteRenderer spriteRenderer;
    Animator anim;

    bool isKnockback = false;

    void Awake()
    {
        rigid = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        anim = GetComponent<Animator>();
    }

    void Update()
    {
        if (isKnockback) return;

        //Jump
        if (Input.GetButtonDown("Jump") && !anim.GetBool("isJumping"))
        {
            rigid.AddForce(Vector2.up * jumpPower, ForceMode2D.Impulse);
            anim.SetBool("isJumping", true);

        }

        //Stop Speed
        if (Input.GetButtonUp("Horizontal"))
        {
            rigid.linearVelocity = new Vector2(rigid.linearVelocity.normalized.x * 0.5f, rigid.linearVelocity.y);
        }

        //Direction Sprite
        if (Input.GetButton("Horizontal"))
        {
            spriteRenderer.flipX = Input.GetAxisRaw("Horizontal") == -1;
        }

        //Walk Animation
        if (Mathf.Abs(rigid.linearVelocity.x) < 1)
        {
            anim.SetBool("isWalking", false);
        }
        else
        {
            anim.SetBool("isWalking", true);
        }
    }

    void FixedUpdate()
    {
        if (isKnockback) return;

        //Move Speed And Key Control 
        float h = Input.GetAxisRaw("Horizontal");


        rigid.AddForce(Vector2.right * h, ForceMode2D.Impulse);

        if (rigid.linearVelocity.x > maxSpeed)
        { //Right max speed
            rigid.linearVelocity = new Vector2(maxSpeed, rigid.linearVelocity.y);
        }

        else if (rigid.linearVelocity.x < maxSpeed * (-1))
        {  //Left max speed
            rigid.linearVelocity = new Vector2(maxSpeed * (-1), rigid.linearVelocity.y);
        }


        //Landing Platform
        if (rigid.linearVelocity.y < 0)
        {
            Debug.DrawRay(rigid.position, Vector3.down, new Color(0, 1, 0));
            RaycastHit2D rayHit = Physics2D.Raycast(rigid.position, Vector3.down, 1, LayerMask.GetMask("Platform"));

            if (rayHit.collider != null)
            {
                if (rayHit.distance < 0.5f)
                {
                    anim.SetBool("isJumping", false);
                    Debug.Log(rayHit.collider.name);
                }
            }
        }

    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Enemy"))
        {
            //공격
            if (rigid.linearVelocity.y < 0 && transform.position.y > collision.transform.position.y)
            {
                OnAttack(collision.transform);
            }

            //피격
            else
            {
                Vector2 contactPoint = collision.contacts[0].point;
                OnDamaged(contactPoint);
            }
        }

        if (isKnockback && collision.gameObject.layer == LayerMask.NameToLayer("Platform"))
        {
            // 버그 방지(이전 호출 취소)
            CancelInvoke("OffDamaged");
            CancelInvoke("OffKnockback");

            // 1. 땅에 닿았으니 넉백 상태를 풀어서 조작이 가능하게 만듦
            Invoke("OffKnockback", 0.3f);

            // 2. 땅에 닿은 지금 이 순간부터 2초 뒤에 무적이 풀리도록 타이머 시작!
            Invoke("OffDamaged", 2);
        }
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Item"))
        {
            //점수
            bool isBronze = collision.gameObject.name.Contains("Bronze");
            bool isSilver = collision.gameObject.name.Contains("Silver");
            bool isGold = collision.gameObject.name.Contains("Gold");

            if(isBronze)
                gameManager.stagePoint += 50;
            else if(isSilver)
                gameManager.stagePoint +=100;
            else if(isGold)
                gameManager.stagePoint += 300;

            //아이템 제거 
            collision.gameObject.SetActive(false);
        }

        else if (collision.gameObject.CompareTag("Finish"))        {
            //다음 스테이지
            gameManager.NextStage();
        }
    }
    void OnAttack(Transform enemy)
    {
        //Point
        gameManager.stagePoint += 100;

        // 반발력
        // 🌟 1. 일단 밑으로 떨어지던 속도(y)를 0으로 멈춰줍니다! (x축 속도는 그대로 유지)
        rigid.linearVelocity = new Vector2(rigid.linearVelocity.x, 0);

        // 🌟 2. 그 상태에서 위로 튕겨 오르는 힘을 줍니다. (5가 약하면 10 정도로 넉넉하게 주세요!)
        rigid.AddForce(Vector2.up * 10, ForceMode2D.Impulse);

        //몬스터 사망
        monstermove enemymove = enemy.GetComponent<monstermove>();
        enemymove.OnDamaged();
    }

    void OnDamaged(Vector2 targetPos)
    {

        gameObject.layer = 11;                                      //피격시 레이어 변경
        spriteRenderer.color = new Color(1, 1, 1, 0.4f);            //피격 반응

        int dirc = transform.position.x - targetPos.x > 0 ? 1 : -1; //피격시 어느쪽으로 밀려날지

        isKnockback = true;                                         //조작 불능 상태 시작

        rigid.linearVelocity = Vector2.zero;                        //피격시 조작감 없애기
        rigid.AddForce(new Vector2(dirc, 1) * 7, ForceMode2D.Impulse); //얼마나 밀려나나?
        anim.SetTrigger("doDamaged");                               //피격애니
    }

    void OffDamaged()
    {
        gameObject.layer = 10;
        spriteRenderer.color = new Color(1, 1, 1, 1);
    }

    void OffKnockback()
    {
        isKnockback = false;
    }

}