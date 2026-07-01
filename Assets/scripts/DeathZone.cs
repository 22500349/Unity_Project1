using UnityEngine;

// 콜라이더 터널링 문제를 피하기 위해 Y 좌표로 직접 낙사 판정
public class DeathZone : MonoBehaviour
{
    public float killY = -8f;  // 이 Y 좌표 아래로 떨어지면 낙사

    float cooldown = 0f;

    void Update()
    {
        if (cooldown > 0f)
        {
            cooldown -= Time.deltaTime;
            return;
        }

        GameManager gm = GameManager.instance;
        if (gm == null) { Debug.LogWarning("DeathZone: GameManager.instance가 null"); return; }
        if (gm.player == null) { Debug.LogWarning("DeathZone: player가 null"); return; }
        if (gm.health <= 0) return;

        float playerY = gm.player.transform.position.y;
        Debug.Log($"플레이어 Y: {playerY:F2}  killY: {killY}");

        if (playerY < killY)
        {
            cooldown = 2f;

            if (gm.health > 1)
                gm.PlayerReporsition();

            gm.HealthDown();
        }
    }
}
