using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class CollisionSensor : BTComponent
{
    public enum CollisionType { Enter, Stay, Exit }
    public struct CollisionInfo
    {
        public Vector2 normal;
        public List<string> tags;
        public Entity entity;
        public Vector2? contactPoint;
        public int frame;
    }

    private List<CollisionInfo> currentCollisions = new List<CollisionInfo>();
    private List<CollisionInfo> exitCollisions = new List<CollisionInfo>();

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (currentCollisions.Any(info => info.entity == collision.collider.GetComponent<Entity>()))
            return;

        AddCollision(currentCollisions, collision);

        Debug.Log($"OnCollisionEnter2D: {collision.collider.name}");
    }

    void OnCollisionStay2D(Collision2D collision)
    {
        if (!currentCollisions.Any(info => info.entity == collision.collider.GetComponent<Entity>()))

            AddCollision(currentCollisions, collision);
    }


    private void OnCollisionExit2D(Collision2D collision)
    {
        // currentCollisions에서 제거
        currentCollisions.RemoveAll(info => info.entity == collision.collider.GetComponent<Entity>());

        AddCollision(exitCollisions, collision);

        Debug.Log($"OnCollisionExit2D: {collision.collider.name}");
    }


    private void AddCollision(List<CollisionInfo> list, Collision2D collision)
    {
        var entity = collision.collider.GetComponent<Entity>();

        List<string> tags = entity != null ? entity.Tags : new List<string>();
        Vector2? contactPoint = null;
        Vector2 normal = Vector2.zero;
        if (collision.contactCount > 0)
        {
            contactPoint = collision.GetContact(0).point;
            normal = collision.GetContact(0).normal;
        }
        list.Add(new CollisionInfo
        {
            normal = normal,
            tags = tags,
            entity = entity,
            contactPoint = contactPoint,
            frame = Time.frameCount
        });
    }

    private void Update()
    {
        // currentCollisions에서 현재 없는 사라진 eneity 제거 (willBeDestroyed) 얘는 exit으로 처리
        List<CollisionInfo> deletedInfos = new List<CollisionInfo>();
        foreach (var info in currentCollisions)
        {
            if (info.entity.WillBeDestroyed)
            {
                var newInfo = info;
                newInfo.frame = Time.frameCount + 1;
                exitCollisions.Add(newInfo);
                deletedInfos.Add(info);
            }
        }

        foreach (var info in deletedInfos)
            currentCollisions.Remove(info);

        exitCollisions.RemoveAll(info => info.frame < Time.frameCount);
    }

    public bool TryGetRecentCollision(string direction, string[] targetTags, string collisionType, out Entity entity)
    {
        entity = null;

        // 현재와 같은 프레임은 enter,
        // 현재 다음프레임은 stay


        if (collisionType == "enter")
        {
            foreach (var info in currentCollisions)
            {
                if (targetTags.Length > 0 && !info.tags.Any(tag => targetTags.Contains(tag)))
                    continue;

                if (Time.frameCount == info.frame && DirectionMatches(direction, info.normal))
                {
                    Debug.Log($"[CollisionSensor] TryGetRecentCollision(enter): {info.entity.name}");
                    entity = info.entity;
                    return true;
                }
            }
        }

        else if (collisionType == "stay")
        {
            foreach (var info in currentCollisions)
            {
                if (targetTags.Length > 0 && !info.tags.Any(tag => targetTags.Contains(tag)))
                    continue;

                if (Time.frameCount != info.frame && DirectionMatches(direction, info.normal))
                {
                    entity = info.entity;
                    return true;
                }
            }
        }

        else if (collisionType == "exit")
        {
            foreach (var info in exitCollisions)
            {
                if (targetTags.Length > 0 && !info.tags.Any(tag => targetTags.Contains(tag)))
                    continue;

                if (DirectionMatches(direction, info.normal))
                {
                    entity = info.entity;
                    return true;
                }
            }
        }

        return false;
    }

    private bool DirectionMatches(string direction, Vector2 normal)
    {
        direction = direction.ToLower();
        switch (direction)
        {
            case "down": return Vector2.Dot(normal, Vector2.up) > 0.7f;
            case "up": return Vector2.Dot(normal, Vector2.down) > 0.7f;
            case "left": return Vector2.Dot(normal, Vector2.right) > 0.7f;
            case "right": return Vector2.Dot(normal, Vector2.left) > 0.7f;
            case "any": return true;
            default: return false;
        }
    }

    private void OnDrawGizmos()
    {
        // 현재 충돌 시각화 (녹색)
        foreach (var info in currentCollisions)
        {
            if (Time.frameCount == info.frame) // enter
                DrawCollisionGizmo(info, Color.green);
            else // stay
                DrawCollisionGizmo(info, Color.yellow);
        }

        // exit 충돌 시각화 (빨간색)
        foreach (var info in exitCollisions)
        {
            DrawCollisionGizmo(info, Color.red);
        }
    }

    private void DrawCollisionGizmo(CollisionInfo info, Color color)
    {
        if (info.contactPoint.HasValue)
        {
            Gizmos.color = color;
            Gizmos.DrawSphere(info.contactPoint.Value, 0.08f);
            Gizmos.DrawLine(info.contactPoint.Value, info.contactPoint.Value + info.normal * 0.5f);
        }
    }
}