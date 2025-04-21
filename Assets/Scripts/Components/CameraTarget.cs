using Newtonsoft.Json.Linq;
using UnityEngine;

public class CameraTarget : IComponent
{
    public static CameraTarget CurrentTarget { private set; get; }

    public override void FromJson(JObject json)
    {
    }

    protected override string GetComponentType() => "CameraTarget";

    public override void Init(Entity entity)
    {
        base.Init(entity);
        CurrentTarget = this;
    }

    private void OnDestroy()
    {
        if (CurrentTarget == this)
        {
            CurrentTarget = null;
        }
    }
}