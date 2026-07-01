using UnityEngine;

[RequireComponent(typeof(BoxCollider2D))]
public class MonsterTrigger : MonoBehaviour
{
    public monstermove[] targets;
    public float jumpForce = 8f;
    public bool repeatOnExit = false;  // true면 플레이어가 나갔다 오면 다시 점프

    bool triggered = false;

    void OnTriggerEnter2D(Collider2D col)
    {
        if (!col.CompareTag("Player")) return;
        if (triggered) return;

        triggered = true;

        foreach (var m in targets)
        {
            if (m == null) continue;
            Rigidbody2D rb = m.GetComponent<Rigidbody2D>();
            if (rb != null)
                rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
        }
    }

    void OnTriggerExit2D(Collider2D col)
    {
        if (repeatOnExit && col.CompareTag("Player"))
            triggered = false;
    }

    public void ResetTrigger()
    {
        triggered = false;
    }

    void OnDrawGizmos()
    {
        if (!TryGetComponent<BoxCollider2D>(out var box)) return;

        Gizmos.matrix = transform.localToWorldMatrix;
        Gizmos.color = new Color(1f, 0.3f, 0.3f, 0.3f);
        Gizmos.DrawCube(box.offset, box.size);
        Gizmos.color = new Color(1f, 0.3f, 0.3f, 1f);
        Gizmos.DrawWireCube(box.offset, box.size);
        Gizmos.matrix = Matrix4x4.identity;
    }
}
