using UnityEngine;
using System.Collections;

[RequireComponent(typeof(BoxCollider2D))]
// 플레이어가 트리거 존을 밟으면 → 지정한 오브젝트를 이동시킵니다.
public class MapTrigger : MonoBehaviour
{
    [Header("움직일 오브젝트")]
    public GameObject target;

    [Header("이동 설정")]
    public Vector2 moveOffset;   // 이동 방향·거리 (예: 위로 3칸 = (0, 3))
    public float speed = 4f;     // 이동 속도
    public float delay = 0f;     // 발동 후 딜레이 (초)

    [Header("옵션")]
    public bool oneShot = true;  // true = 한 번만 발동 / false = 반복 발동
    public bool returnOnExit = false; // true = 플레이어가 나가면 원위치

    Vector2 originPos;
    Vector2 destPos;
    bool triggered = false;

    void Start()
    {
        if (target == null) return;
        originPos = target.transform.position;
        destPos = originPos + moveOffset;
    }

    void OnTriggerEnter2D(Collider2D col)
    {
        if (!col.CompareTag("Player")) return;
        if (oneShot && triggered) return;

        triggered = true;
        StopAllCoroutines();
        StartCoroutine(MoveTo(destPos));
    }

    void OnTriggerExit2D(Collider2D col)
    {
        if (!returnOnExit || !col.CompareTag("Player")) return;
        StopAllCoroutines();
        StartCoroutine(MoveTo(originPos));
        triggered = false;
    }

    IEnumerator MoveTo(Vector2 goal)
    {
        if (delay > 0f)
            yield return new WaitForSeconds(delay);

        while (target != null && Vector2.Distance(target.transform.position, goal) > 0.01f)
        {
            target.transform.position = Vector2.MoveTowards(
                target.transform.position, goal, speed * Time.deltaTime);
            yield return null;
        }

        if (target != null)
            target.transform.position = goal;
    }

    void OnDrawGizmos()
    {
        if (!TryGetComponent<BoxCollider2D>(out var box)) return;

        Gizmos.matrix = transform.localToWorldMatrix;
        Gizmos.color = new Color(0.3f, 0.8f, 1f, 0.3f);
        Gizmos.DrawCube(box.offset, box.size);
        Gizmos.color = new Color(0.3f, 0.8f, 1f, 1f);
        Gizmos.DrawWireCube(box.offset, box.size);
        Gizmos.matrix = Matrix4x4.identity;
    }
}
