using Newtonsoft.Json.Linq;

public class Gravity_ : ComponentUsingRigidbody2D
{
    private float gravityScale = 0;

    public void SetGravityScale(float gravityScale)
    {
        CheckRigidbody2D();

        this.gravityScale = gravityScale;
        rigidbody2D.gravityScale = gravityScale;
    }

    public float GetGravityScale()
    {
        return gravityScale;
    }

    public override JObject ToJson()
    {
        var json = base.ToJson();
        json.Add("gravityScale", GetGravityScale());
        return json;
    }

    public override void FromJson(JObject json)
    {
        if (!IsCorrectJson(json)) return;

        float value = json["gravityScale"].Value<float>();
        SetGravityScale(value);
    }

    protected override string GetComponentType() => "Gravity";
}
