using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class TitleManager : MonoBehaviour
{
    public Image panel;
    
    // 추가: 오디오 소스를 연결할 변수
    [Header("사운드 설정")]
    public AudioSource startSound; 

    IEnumerator Fadeout()
    {
        Color color = panel.color;

        while (color.a > 0)
        {
            color.a -= Time.deltaTime;
            panel.color = color;
            yield return null;
        }
    }

    IEnumerator Fadein()
    {
        Color color = panel.color;

        while (color.a < 1)
        {
            color.a += Time.deltaTime;
            panel.color = color;
            yield return null;
        }
    }

    IEnumerator StartGameCoroutine()
    {
        // 추가: 코루틴이 시작될 때(버튼을 누른 직후) 소리를 재생합니다.
        if (startSound != null)
        {
            startSound.Play();
        }

        panel.gameObject.SetActive(true);
        
        // 페이드인 연출(약 1초)이 끝날 때까지 대기
        yield return StartCoroutine(Fadein());
        
        // 페이드인이 끝나면 씬 전환
        SceneManager.LoadScene("Stage1");
    }

    void Awake()
    {
        panel.gameObject.SetActive(false);
    }

    public void StartGame()
    {
        StartCoroutine(StartGameCoroutine());
    }
}