using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using UnityEngine;

public class TimerDestroy : IComponent
{
    [Header("Read Only!")]
    [SerializeField]
    private float time;

    public override void Init(Entity entity)
    {
        base.Init(entity);
        StartCoroutine(nameof(TimerRoutine));
    }

    private IEnumerator TimerRoutine()
    {
        yield return new WaitForSeconds(time);
        Destroy(entity.gameObject);
    }

    public override JObject ToJson()
    {
        var json = base.ToJson();
        json.Add("time", time);
        return json;
    }

    public override void FromJson(JObject json)
    {
        time = json["time"].Value<float>();
    }

    protected override string GetComponentType()
    {
        throw new System.NotImplementedException();
    }
}
