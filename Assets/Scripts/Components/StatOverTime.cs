using System.Collections;
using Newtonsoft.Json.Linq;
using UnityEngine;

public class StatOverTime : IComponent
{
    [SerializeField] private string stat = "";             // 변경할 stat 이름
    [SerializeField] private float amountPerSecond = 0f;   // 초당 변화량 (음수는 감소)
    [SerializeField] private float interval = 1f;          // 업데이트 주기 (초)

    private Coroutine statChangeCoroutine;

    public override void Init(Entity entity)
    {
        base.Init(entity);

        // 시작 시 Coroutine 시작
        statChangeCoroutine = entity.StartCoroutine(ChangeStatOverTime());
    }

    // 주기적으로 Stat을 변경하는 Coroutine
    private IEnumerator ChangeStatOverTime()
    {
        while (true)
        {
            yield return new WaitForSeconds(interval);  // interval마다 실행
            print("aa");

            // Stat을 변경
            if (!string.IsNullOrEmpty(stat))
            {
                var stats = GetIComponent<Stats>();
                if (stats != null)
                {
                    // Stat 값 변경: amountPerSecond만큼 더하기 또는 빼기
                    float currentValue = stats.GetStat(stat);
                    stats.SetStat(stat, currentValue + amountPerSecond);
                }
            }
        }
    }

    public override void FromJson(JObject json)
    {
        if (json.TryGetValue("stat", out var statToken))
            stat = statToken.Value<string>();

        if (json.TryGetValue("amountPerSecond", out var amountToken))
            amountPerSecond = amountToken.Value<float>();

        if (json.TryGetValue("interval", out var intervalToken))
            interval = intervalToken.Value<float>();
    }

    public override JObject ToJson()
    {
        var json = base.ToJson();
        if (!string.IsNullOrEmpty(stat)) json["stat"] = stat;
        json["amountPerSecond"] = amountPerSecond;
        json["interval"] = interval;
        return json;
    }

    protected override string GetComponentType() => "StatOverTime";
}