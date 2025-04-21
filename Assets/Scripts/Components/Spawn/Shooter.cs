using System;
using Newtonsoft.Json.Linq;
using UnityEngine;

public class Shooter : IComponent
{
    [SerializeField]
    private string projectilePresetId = "";

    [SerializeField]
    private int listenTo = -1;

    [SerializeField]
    private string shootDirection = "facing"; // "facing" or "fixed"

    [SerializeField]
    private Vector2 fixedDirection = Vector2.right;

    [SerializeField]
    private string directionSpace = "local"; // "local" or "world"

    public void Shoot()
    {
        if (string.IsNullOrEmpty(projectilePresetId)) return;

        Entity clone = ClonePreset(projectilePresetId, transform.position);
        Projectile projectile = clone.GetIComponent<Projectile>();

        Physics2D.IgnoreCollision(GetComponent<Collider2D>(), projectile.GetComponent<Collider2D>());

        if (projectile != null)
            projectile.SetDirection(CalculateShootDirection());
    }

    private Vector2 CalculateShootDirection()
    {
        return shootDirection switch
        {
            "facing" => transform.right,
            "fixed" => directionSpace == "world" ? fixedDirection.normalized : transform.TransformDirection(fixedDirection.normalized),
            _ => transform.right
        };
    }

    public override void FromJson(JObject json)
    {
        if (json.TryGetValue("projectilePresetId", out var presetToken))
            projectilePresetId = presetToken.Value<string>();

        if (json.TryGetValue("listenTo", out var listenToToken))
        {
            listenTo = listenToToken.Value<int>();
            ControllerButton controller = FindControllerByIndex<ControllerButton>(listenTo);
            if (controller)
            {
                controller.OnGetKeyDown.RemoveListener(Shoot);
                controller.OnGetKeyDown.AddListener(Shoot);
            }
        }

        if (json.TryGetValue("shootDirection", out var dirToken))
            shootDirection = dirToken.Value<string>();

        if (json.TryGetValue("fixedDirection", out var fixedToken) && fixedToken is JArray arr && arr.Count == 2)
        {
            fixedDirection = new Vector2(arr[0].Value<float>(), arr[1].Value<float>());
        }

        if (json.TryGetValue("directionSpace", out var spaceToken))
            directionSpace = spaceToken.Value<string>();
    }

    public override JObject ToJson()
    {
        var json = base.ToJson();
        json["projectilePresetId"] = projectilePresetId;
        json["listenTo"] = listenTo;
        json["shootDirection"] = shootDirection;

        if (shootDirection == "fixed")
        {
            json["fixedDirection"] = new JArray { fixedDirection.x, fixedDirection.y };
            json["directionSpace"] = directionSpace;
        }

        return json;
    }

    protected override string GetComponentType() => "Shooter";
}