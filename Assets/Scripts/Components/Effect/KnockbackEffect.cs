using Newtonsoft.Json.Linq;
using UnityEngine;

public class KnockbackEffect : IEffect
{
    public float Force;
    public string Direction;           // "facing", "fromSelf", "fixed"
    public Vector2 FixedDirection;
    public string Space;               // "world", "local"

    public string Type => "knockback";

    public JObject ToJson()
    {
        return new JObject
        {
            ["type"] = Type,
            ["force"] = Force,
            ["direction"] = Direction,
            ["fixedDirection"] = new JArray(FixedDirection.x, FixedDirection.y),
            ["space"] = Space
        };
    }

    public void FromJson(JObject json)
    {
        Force = json["force"]?.Value<float>() ?? 0;
        Direction = json["direction"]?.ToString();
        Space = json["space"]?.ToString();

        if (json["fixedDirection"] is JArray arr && arr.Count == 2)
        {
            FixedDirection = new Vector2(arr[0].Value<float>(), arr[1].Value<float>());
        }
    }
}