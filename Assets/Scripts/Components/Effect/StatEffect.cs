using Newtonsoft.Json.Linq;

public class StatEffect : IEffect
{
    public string Key;
    public float Value;

    public string Type => "stat";

    public JObject ToJson()
    {
        return new JObject
        {
            ["type"] = Type,
            ["key"] = Key,
            ["value"] = Value
        };
    }

    public void FromJson(JObject json)
    {
        Key = json["key"]?.ToString();
        Value = json["value"]?.Value<float>() ?? 0f;
    }
}