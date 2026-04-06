using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;
public class GameManager : MonoBehaviour
{
    public int totalPoint = 0;
    public int stagePoint;
    public int stageIndex;
    public int health;
    public PlayerMove player;
    public GameObject[] stages;

    public Image[] UIhealth;
    public TextMeshProUGUI UI_point;
    public TextMeshProUGUI UI_stage;
    public GameObject RestartBtn;

    // Start is called once before the first execution of Update after the MonoBehaviour is created

    void Update()
    {
        UI_point.text = (totalPoint + stagePoint).ToString();
    }
    public void NextStage()
    {
        //스테이지 변경
        if (stageIndex < stages.Length - 1)
        {
            stages[stageIndex].SetActive(false);
            stageIndex++;
            stages[stageIndex].SetActive(true);

            // 마지막 스테이지(축하 맵)에 도달했는지 확인
            if (stageIndex == stages.Length - 1)
            {
                UI_stage.text = "CLEAR!";
                Debug.Log("게임 클리어!");

                // [개선 포인트 3] 게임 인터페이스를 TMPro로 통일하셨기 때문에, 버튼 안의 글씨도 Text 대신 
                // TextMeshProUGUI를 찾아와야 에러(NullReferenceException) 없이 텍스트를 바꿀 수 있습니다!
                RestartBtn.SetActive(true);
                TextMeshProUGUI btnText = RestartBtn.GetComponentInChildren<TextMeshProUGUI>();
                if (btnText != null)
                {
                    btnText.text = "GAME CLEAR";
                }
            }
            else
            {
                UI_stage.text = "STAGE" + (stageIndex + 1);
            }
        }

        else
        {   //게임 클리어!
            Debug.Log("게임 클리어!");
            RestartBtn.SetActive(true);
        }

        //플레이어 위치 초기화
        PlayerReporsition();

        //점수 환산
        totalPoint += stagePoint;
        stagePoint = 0;
    }

    public void HealthDown()
    {
        if (health > 1)
        {
            health--;
            UIhealth[health].color = new Color(1, 0, 0, 0.4f);
        }

        else
        {
            // [주의] 이미 사망했는데 바닥으로 다시 떨어져 OnTriggerEnter2D가 또 불리면, 
            // OnDie가 반복 호출되어 위로 계속 튕겨오르는 버그가 발생할 수 있습니다. 
            // bool isDead 같은 변수를 두고 이미 죽었으면 예외처리하는 것이 안전합습니다.

            UIhealth[0].color = new Color(1, 0, 0, 0.4f);

            //플레이어 사망 이펙트    
            player.OnDie();

            //결과 화면
            Debug.Log("Game Over");
            RestartBtn.SetActive(true);


        }
    }

    // Update is called once per frame
    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            //플레이어 사망시 트리거 발동 방지
            if (health > 1)
            {
                PlayerReporsition();
            }

            //낙하시 피 감소
            HealthDown();
        }
    }

    void PlayerReporsition()
    {
        //낙하시 또는 다음 스테이지로 넘어갈 때 원상복귀될 위치 좌표
        player.transform.position = new Vector3(1, 3, -1);
        player.VelocityZero();

    }

    // 재시작 버튼에서 호출할 함수
    public void Restart()
    {
        Time.timeScale = 1;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}
