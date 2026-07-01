using UnityEngine;

// 낙사 감지 트리거 - 씬마다 배치. GameManager에서 분리.
public class DeathZone : MonoBehaviour
{
    void OnTriggerEnter2D(Collider2D collision)
    {
        if (!collision.gameObject.CompareTag("Player")) return;

        GameManager gm = GameManager.instance;
        if (gm.health > 1)
            gm.PlayerReporsition();
        gm.HealthDown();
    }
}
