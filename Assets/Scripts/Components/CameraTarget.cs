using Newtonsoft.Json.Linq;
using UnityEngine;

public class CameraTarget : IComponent
{
    public static CameraTarget CurrentTarget;

    public override void FromJson(JObject json)
    {
    }

    protected override string GetComponentType() => "CameraTarget";

    private void Awake()
    {
        // 하나의 타겟만 존재하도록 보장
        if (CurrentTarget == null)
        {
            CurrentTarget = this;
        }
        else
        {
            Debug.LogWarning("Multiple CameraTarget components detected. Only the first will be used.");
        }
    }

    private void OnDestroy()
    {
        if (CurrentTarget == this)
        {
            CurrentTarget = null;
        }
    }
}