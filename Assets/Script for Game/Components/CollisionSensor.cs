using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class CollisionSensor : MonoBehaviour
{
    public enum CollisionType { Enter, Stay, Exit }
    public struct CollisionInfo
    {
        public Vector2 normal;
        public List<string> tags;
        public Entity entity;
        public Collision2D collision;
        public Vector2? contactPoint;
    }

    private List<CollisionInfo> enterCollisions = new List<CollisionInfo>();
    private List<CollisionInfo> exitCollisions = new List<CollisionInfo>();
    private Dictionary<Entity, CollisionInfo> stayCollisionDict = new Dictionary<Entity, CollisionInfo>();
    private const float collisionMemoryTime = 0.2f; // seconds
    private float lastEnterTime = -1f, lastExitTime = -1f;

    private void OnCollisionEnter2D(Collision2D collision)
    {
        AddCollision(enterCollisions, collision, ref lastEnterTime);
    }
    private void OnCollisionStay2D(Collision2D collision)
    {
        var entity = collision.collider.GetComponent<Entity>();
        if (entity == null) return;
        Vector2? contactPoint = null;
        Vector2 normal = Vector2.zero;
        if (collision.contactCount > 0)
        {
            contactPoint = collision.GetContact(0).point;
            normal = collision.GetContact(0).normal;
        }
        var info = new CollisionInfo
        {
            normal = normal,
            tags = entity.Tags,
            entity = entity,
            collision = collision,
            contactPoint = contactPoint
        };
        stayCollisionDict[entity] = info;
    }
    private void OnCollisionExit2D(Collision2D collision)
    {
        var entity = collision.collider.GetComponent<Entity>();
        if (entity != null)
            stayCollisionDict.Remove(entity);
        AddCollision(exitCollisions, collision, ref lastExitTime);
    }

    private void AddCollision(List<CollisionInfo> list, Collision2D collision, ref float lastTime)
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
            collision = collision,
            contactPoint = contactPoint
        });
        lastTime = Time.time;
    }

    private void Update()
    {
        // 오래된 충돌 정보 삭제
        if (enterCollisions.Count > 0 && Time.time - lastEnterTime > collisionMemoryTime)
            enterCollisions.Clear();
        if (exitCollisions.Count > 0 && Time.time - lastExitTime > collisionMemoryTime)
            exitCollisions.Clear();
        // stayCollisionDict는 exit에서만 제거
    }

    public bool TryGetRecentCollision(string direction, string[] targetTags, string collisionType, out Entity entity)
    {
        entity = null;
        if (collisionType == "stay")
        {
            foreach (var info in stayCollisionDict.Values)
            {
                if (targetTags.Length > 0 && !info.tags.Any(tag => targetTags.Contains(tag)))
                    continue;
                if (DirectionMatches(direction, info.normal))
                {
                    entity = info.entity;
                    return true;
                }
            }
            return false;
        }
        List<CollisionInfo> list = enterCollisions;
        if (collisionType == "exit") list = exitCollisions;
        foreach (var info in list)
        {
            if (targetTags.Length > 0 && !info.tags.Any(tag => targetTags.Contains(tag)))
                continue;
            if (DirectionMatches(direction, info.normal))
            {
                entity = info.entity;
                return true;
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
        // 가장 최근의 각 타입별 충돌만 시각화
        if (enterCollisions.Count > 0)
            DrawCollisionGizmo(enterCollisions[enterCollisions.Count - 1], Color.green);
        if (stayCollisionDict.Count > 0)
        {
            foreach (var info in stayCollisionDict.Values)
                DrawCollisionGizmo(info, Color.yellow);
        }
        if (exitCollisions.Count > 0)
            DrawCollisionGizmo(exitCollisions[exitCollisions.Count - 1], Color.red);
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