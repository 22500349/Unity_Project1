using System.Collections;
using UnityEngine;
using UnityEngine.Tilemaps; 

public class DisappearingPlatform : MonoBehaviour
{
    [Header("시간 설정 (초)")]
    public float delayTime = 2f;       // 밟고 사라질 때까지의 시간
    public float respawnTime = 3f;     // 사라진 후 자동으로 다시 나타나는 시간

    private TilemapRenderer tilemapRenderer;
    private TilemapCollider2D tilemapCollider;
    private bool isTriggered = false;

    private void Awake()
    {
        // 끄고 켤 컴포넌트들을 연결합니다.
        tilemapRenderer = GetComponent<TilemapRenderer>();
        tilemapCollider = GetComponent<TilemapCollider2D>();
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player") && !isTriggered)
        {
            isTriggered = true;
            StartCoroutine(DisappearSequence());
        }
    }

    private IEnumerator DisappearSequence()
    {
        // 1. 지정된 시간(2초) 대기
        yield return new WaitForSeconds(delayTime);

        // 2. 플랫폼 숨기기
        SetPlatformState(false);

        // 3. 지정된 시간(3초) 대기 (이 부분이 활성화되었습니다!)
        yield return new WaitForSeconds(respawnTime);

        // 4. 플랫폼 다시 나타나게 복구
        ResetPlatform();
    }

    private void SetPlatformState(bool visible)
    {
        if (tilemapRenderer != null) tilemapRenderer.enabled = visible;
        if (tilemapCollider != null) tilemapCollider.enabled = visible;
    }

    public void ResetPlatform()
    {
        SetPlatformState(true); // 다시 보이게 설정
        isTriggered = false;    // 다시 밟을 수 있게 상태 초기화
    }
}