using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class Movement : BTComponent
{
    private new Rigidbody2D rigidbody2D;
    private new Collider2D collider2D;

    public Vector2 Velocity { get { return rigidbody2D.velocity; } }

    protected override void FindComponents()
    {
        // Destroy the existing box collider and add a circle collider.
        Destroy(GetComponent<Collider2D>());
        gameObject.AddComponent<CircleCollider2D>();

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
}
