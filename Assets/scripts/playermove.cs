using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(SpriteRenderer))]
[RequireComponent(typeof(Animator))]
// [개선 포인트 1] AudioSource 컴포넌트를 코드로 다루기 때문에 RequireComponent를 달아두면 
// 실수로 컴포넌트를 지워 발생하는 Null 에러를 방지할 수 있습니다!
[RequireComponent(typeof(AudioSource))]

public class PlayerMove : MonoBehaviour
{
    public GameManager gameManager;

    //오디오
    public AudioClip audioJump;
    public AudioClip audioAttack;
    public AudioClip audioDamage;
    public AudioClip audioItem;
    public AudioClip audioDie;
    public AudioClip audioFinish;

    //게임 기본 설정
    public float maxSpeed;
    public float jumpPower;

    //컴포넌트
    Rigidbody2D rigid;
    SpriteRenderer spriteRenderer;
    Animator anim;
    CapsuleCollider2D capsuleCollider;
    AudioSource audioSource;

    bool isKnockback = false;

    void Awake()
    {
        rigid = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        capsuleCollider = GetComponent<CapsuleCollider2D>();
        anim = GetComponent<Animator>();
        audioSource = GetComponent<AudioSource>();
    }



    void Update()
    {
        if (isKnockback) return;

        //Jump
        if (Input.GetButtonDown("Jump") && !anim.GetBool("isJumping"))
        {
            rigid.AddForce(Vector2.up * jumpPower, ForceMode2D.Impulse);
            anim.SetBool("isJumping", true);
            PlaySound("Jump");
        }

        //Stop Speed
        if (Input.GetButtonUp("Horizontal"))
        {
            // [주의] rigid.linearVelocity.normalized.x 는 속도가 0에 수렴할 때 방향을 잃어 의도치 않게 작동할 수 있습니다.
            // 단순히 방향 유지 외에 감속을 원하시는 거라면 rigid.linearVelocity.x * 0.5f (본래 속도의 절반)이 더 부드럽습니다.
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
        // 점프 상승 중이 아닐 때 착지로 판정합니다. (비탈길에서 velocity.y가 0~약한 양수가 되는 것 대비)
        if (rigid.linearVelocity.y <= 0.1f)
        {
            // 비탈길에서는 발끝-지면까지의 거리가 수직보다 길어지므로 여유를 더해줍니다.(+0.2f)
            float rayDistance = capsuleCollider.bounds.extents.y + 0.2f;

            // 콜라이더 좌/우 끝에서 쏘기 위한 여유분
            float offset = capsuleCollider.bounds.extents.x * 0.8f;

            Vector2 pos = rigid.position;
            Vector2 leftPos = new Vector2(pos.x - offset, pos.y);
            Vector2 rightPos = new Vector2(pos.x + offset, pos.y);

            // 시각적 확인
            Debug.DrawRay(pos, Vector3.down * rayDistance, new Color(0, 1, 0));
            Debug.DrawRay(leftPos, Vector3.down * rayDistance, new Color(0, 1, 0));
            Debug.DrawRay(rightPos, Vector3.down * rayDistance, new Color(0, 1, 0));

            // 가운데, 좌측, 우측 세 곳에서 지형 감지
            RaycastHit2D hitCenter = Physics2D.Raycast(pos, Vector3.down, rayDistance, LayerMask.GetMask("Platform"));
            RaycastHit2D hitLeft = Physics2D.Raycast(leftPos, Vector3.down, rayDistance, LayerMask.GetMask("Platform"));
            RaycastHit2D hitRight = Physics2D.Raycast(rightPos, Vector3.down, rayDistance, LayerMask.GetMask("Platform"));

            if (hitCenter.collider != null || hitLeft.collider != null || hitRight.collider != null)
            {
                anim.SetBool("isJumping", false);
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
                PlaySound("Attack");
            }

            //피격
            else
            {
                Vector2 contactPoint = collision.contacts[0].point;
                OnDamaged(contactPoint);
                PlaySound("Damage");
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

            if (isBronze)
                gameManager.stagePoint += 50;
            else if (isSilver)
                gameManager.stagePoint += 100;
            else if (isGold)
                gameManager.stagePoint += 300;

            //아이템 제거 
            collision.gameObject.SetActive(false);
            PlaySound("Item");
        }

        else if (collision.gameObject.CompareTag("Finish"))
        {
            // [버그 방지] 콜라이더나 여러 요인으로 한 번에 여러 번 닿아 
            // 스테이지가 순식간에 넘어가 클리어되는 것을 방지하기 위해 비활성화합니다.
            collision.enabled = false;

            //다음 스테이지
            gameManager.NextStage();
            PlaySound("Finish");
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
        if (enemymove != null)
        {
            enemymove.OnDamaged();
        }
    }

    public void OnDamaged(Vector2 targetPos)
    {
        gameManager.HealthDown();                                   // 피 감소

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

    public void OnDie()
    {
        // [주의] 게임오버 상태인데 외부(낙하 트리거 등)에서 여러 번 호출되면 AddForce가 계속 누적되어 캐릭터가 통통 튑니다.
        // "if (상태 == 사망) return;" 과 같은 이중 실행 방지 방어 코드가 필요합니다.

        //sprite Alpha
        spriteRenderer.color = new Color(1, 1, 1, 0.4f);
        //Sprite Flip
        spriteRenderer.flipY = true;
        //Collider Disable
        capsuleCollider.enabled = false;
        //Die Effect Jump
        rigid.AddForce(Vector2.up * 5, ForceMode2D.Impulse);
        PlaySound("Die");

    }

    public void VelocityZero()
    {
        rigid.linearVelocity = Vector2.zero;
    }

    // [개선 포인트 2] audioSource.Play()는 이미 소리가 나고 있을 때 다른 소리를 재생하면 
    // 이전 소리를 뚝 끊어버리는 단점이 있습니다! (예: 코인 여러 개를 동시에 먹을 때 소리가 끊김)
    // 대신 PlayOneShot()을 쓰면 소리들이 끊기지 않고 자연스럽게 겹쳐서 납니다.
    void PlaySound(string action)
    {
        switch (action)
        {
            case "Jump":
                audioSource.PlayOneShot(audioJump);
                break;
            case "Attack":
                audioSource.PlayOneShot(audioAttack);
                break;
            case "Damage":
                audioSource.PlayOneShot(audioDamage);
                break;
            case "Item":
                audioSource.PlayOneShot(audioItem);
                break;
            case "Die":
                audioSource.PlayOneShot(audioDie);
                break;
            case "Finish":
                audioSource.PlayOneShot(audioFinish);
                break;
        }
    }
}