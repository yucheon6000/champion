using Newtonsoft.Json.Linq;
using UnityEngine;

public class Rotatable : ComponentUsingMovable
{
    [SerializeField]
    private bool faceMovement = false;

    [SerializeField]
    private float turnSpeed = 5000f; // degrees per second

    private float targetAngle;

    public override void Init(Entity entity)
    {
        base.Init(entity);
        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        if (rb)
            rb.constraints &= ~RigidbodyConstraints2D.FreezeRotation;
    }

    public void FaceDirection(Vector2 direction)
    {
        if (direction.sqrMagnitude < 0.001f) return;
        targetAngle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
    }

    private void Update()
    {
        CheckMovable();

        if (faceMovement && movable != null)
        {
            FaceDirection(movable.MoveDirection);
        }

        float currentZ = transform.eulerAngles.z;

        if (turnSpeed < 0f)
        {
            // 즉시 회전
            transform.rotation = Quaternion.Euler(0, 0, targetAngle);
        }
        else
        {
            float angle = Mathf.LerpAngle(currentZ, targetAngle, Time.deltaTime * turnSpeed / 360f);
            transform.rotation = Quaternion.Euler(0, 0, angle);
        }
    }

    public override void FromJson(JObject json)
    {
        if (json.TryGetValue("turnSpeed", out var speedToken))
            turnSpeed = speedToken.Value<float>();

        if (json.TryGetValue("faceMovement", out var faceToken))
            faceMovement = faceToken.Value<bool>();
    }

    public override JObject ToJson()
    {
        var json = base.ToJson();
        json["turnSpeed"] = turnSpeed;
        json["faceMovement"] = faceMovement;
        return json;
    }

    protected override string GetComponentType() => "Rotatable";
}