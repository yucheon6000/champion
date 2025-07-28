using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class Movement : MonoBehaviour
{
    private new Rigidbody2D rigidbody2D;
    private new Collider2D collider2D;

    public Vector2 Velocity { get { return rigidbody2D.velocity; } }

    private void Start()
    {
        rigidbody2D = GetComponent<Rigidbody2D>();
        collider2D = GetComponent<Collider2D>();

        rigidbody2D.constraints = RigidbodyConstraints2D.FreezeRotation;
    }

    public void Move(Vector2 direction, float speed)
    {
        rigidbody2D.velocity = new Vector2(direction.x * speed, rigidbody2D.velocity.y);
    }

    public void Jump(float jumpForce)
    {
        rigidbody2D.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
    }

    public bool IsOnGround()
    {
        if (collider2D == null)
            collider2D = GetComponent<Collider2D>();

        Bounds bounds = collider2D.bounds;
        float minX = bounds.min.x + 0.05f;
        float maxX = bounds.max.x - 0.05f;
        float y = bounds.min.y - 0.01f; // 더 가까운 거리로 수정

        int rayCount = 5;
        float rayLength = 0.08f; // 짧은 거리로 수정 (0.1f → 0.08f)
        int hitCount = 0;

        for (int i = 0; i < rayCount; i++)
        {
            float t = (float)i / (rayCount - 1);
            float x = Mathf.Lerp(minX, maxX, t);
            Vector2 origin = new Vector2(x, bounds.center.y);
            RaycastHit2D[] hits = Physics2D.RaycastAll(origin, Vector2.down, bounds.extents.y + rayLength);

            foreach (var hit in hits)
            {
                if (hit.collider != null && hit.collider != collider2D)
                {
                    Entity entity = hit.collider.GetComponent<Entity>();
                    if (entity != null && entity.HasTag("Ground"))
                    {
                        hitCount++;
                        break;
                    }
                }
            }
        }
        // 여러 레이 중 절반 이상이 닿아야 땅으로 간주 (더 안정적)
        return hitCount >= (rayCount / 2 + 1);
    }

    /*
    private void OnDrawGizmos()
    {
        if (collider2D == null)
            collider2D = GetComponent<Collider2D>();

        Bounds bounds = collider2D.bounds;
        float minX = bounds.min.x + 0.05f;
        float maxX = bounds.max.x - 0.05f;
        float y = bounds.min.y - 0.01f;

        int rayCount = 5;
        float rayLength = 0.08f;
        int hitCount = 0;

        for (int i = 0; i < rayCount; i++)
        {
            float t = (float)i / (rayCount - 1);
            float x = Mathf.Lerp(minX, maxX, t);
            Vector2 origin = new Vector2(x, bounds.center.y);
            RaycastHit2D[] hits = Physics2D.RaycastAll(origin, Vector2.down, bounds.extents.y + rayLength);

            bool rayHitGround = false;
            foreach (var hit in hits)
            {
                if (hit.collider != null && hit.collider != collider2D)
                {
                    Entity entity = hit.collider.GetComponent<Entity>();
                    if (entity != null && entity.HasTag("Ground"))
                    {
                        rayHitGround = true;
                        hitCount++;
                        break;
                    }
                }
            }

            Gizmos.color = rayHitGround ? Color.green : Color.red;
            Gizmos.DrawLine(origin, origin + Vector2.down * (bounds.extents.y + rayLength));
        }

        // 전체 판정 결과를 원으로 표시
        bool isOnGround = hitCount >= (rayCount / 2 + 1);
        Gizmos.color = isOnGround ? Color.green : Color.red;
        Gizmos.DrawWireSphere(new Vector2(bounds.center.x, bounds.min.y), 0.07f);
    }
    */
}
