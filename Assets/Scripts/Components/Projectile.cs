using Newtonsoft.Json.Linq;
using UnityEngine;

public class Projectile : ComponentUsingMovable
{
    public override void Init(Entity entity)
    {
        base.Init(entity);
        // GetComponent<Collider2D>().isTrigger = true;
    }

    private Vector2 direction;

    public void SetDirection(Vector2 direction)
    {
        this.direction = direction;
    }

    private void Update()
    {
        CheckMovable();
        movable.Move(direction);
    }

    public override void FromJson(JObject json) { }

    protected override string GetComponentType() => "Projectile";
}
