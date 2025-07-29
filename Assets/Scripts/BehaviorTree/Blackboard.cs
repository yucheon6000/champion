using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json.Linq;
using Unity.VisualScripting; // JObject와 JToken을 사용하기 위해 추가

public class Blackboard
{
    // --- 글로벌 변수 저장용 static 딕셔너리 ---
    private static readonly Dictionary<string, object> globalData = new Dictionary<string, object>();

    // 모든 데이터를 key(문자열)와 value(오브젝트) 쌍으로 저장
    private readonly Dictionary<string, object> data = new Dictionary<string, object>();

    /// <summary>
    /// 글로벌 변수(JObject)에서 값을 초기화합니다.
    /// </summary>
    public static void SetGlobalFromJson(JObject globalVariablesObj)
    {
        globalData.Clear();
        foreach (var pair in globalVariablesObj)
        {
            string key = pair.Key;
            JToken valueToken = pair.Value;

            // 타입 접두사에 따라 변환
            if (key.StartsWith("g_i_"))
                globalData[key] = valueToken.Value<int>();
            else if (key.StartsWith("g_f_"))
                globalData[key] = valueToken.Value<float>();
            else if (key.StartsWith("g_b_"))
                globalData[key] = valueToken.Value<bool>();
            else if (key.StartsWith("g_s_"))
                globalData[key] = valueToken.Value<string>();
            else
                globalData[key] = valueToken.ToObject<object>();
        }
    }

    /// <summary>
    /// 글로벌 변수에 값을 설정합니다.
    /// </summary>
    public static void SetGlobalValue<T>(string key, T value)
    {
        globalData[key] = value;
    }

    /// <summary>
    /// 글로벌 변수에서 값을 가져옵니다.
    /// </summary>
    public static T GetGlobalValue<T>(string key, T defaultValue = default)
    {
        if (globalData.TryGetValue(key, out object value))
        {
            if (value is T typedValue)
                return typedValue;
            else
            {
                Debug.LogWarning($"Global Blackboard key '{key}'의 타입({value.GetType().Name})이 요청한 타입({typeof(T).Name})과 일치하지 않습니다.");
                return defaultValue;
            }
        }
        return defaultValue;
    }

    /// <summary>
    /// 블랙보드에 특정 키가 존재하는지 확인합니다.
    /// </summary>
    public bool Has(string key)
    {
        return data.ContainsKey(key);
    }

    /// <summary>
    /// 제네릭을 사용하여 모든 타입의 값을 설정합니다.
    /// </summary>
    public void SetValue<T>(string key, T value)
    {
        // 글로벌 변수면 글로벌에 저장
        if (key.StartsWith("g_"))
            SetGlobalValue(key, value);
        else
            data[key] = value;
    }

    /// <summary>
    /// 제네릭을 사용하여 모든 타입의 값을 가져옵니다.
    /// 키가 없거나 타입이 맞지 않으면 지정된 기본값을 반환합니다.
    /// </summary>
    public T GetValue<T>(string key, T defaultValue = default)
    {
        // 글로벌 변수면 글로벌에서 가져옴
        if (key.StartsWith("g_"))
            return GetGlobalValue<T>(key, defaultValue);

        if (data.TryGetValue(key, out object value))
        {
            // Entity는 기본값이 null이므로 타입이 맞지 않으면 기본값을 반환
            if (typeof(T) == typeof(Entity) && value is null)
                return defaultValue;

            // 저장된 값의 타입이 요청한 타입 T와 일치하는지 확인
            if (value is T typedValue)
            {
                return typedValue;
            }
            else
            {
                // 타입이 맞지 않으면 경고를 출력하고 기본값을 반환
                Debug.LogWarning($"Blackboard key '{key}'의 타입({value.GetType().Name})이 요청한 타입({typeof(T).Name})과 일치하지 않습니다.");
                return defaultValue;
            }
        }

        // 키가 존재하지 않으면 기본값을 반환
        return defaultValue;
    }

    /// <summary>
    /// JSON 객체에서 변수를 설정합니다.
    /// </summary>
    public void SetFromJson(JObject variablesObj)
    {
        foreach (var pair in variablesObj)
        {
            string key = pair.Key;
            JToken valueToken = pair.Value;

            // 타입 접두사에 따라 변환
            if (key.StartsWith("i_"))
                SetValue<int>(key, valueToken.Value<int>());
            else if (key.StartsWith("f_"))
                SetValue<float>(key, valueToken.Value<float>());
            else if (key.StartsWith("b_"))
                SetValue<bool>(key, valueToken.Value<bool>());
            else if (key.StartsWith("s_"))
                SetValue<string>(key, valueToken.Value<string>());
            else if (key.StartsWith("e_"))
                SetValue<Entity>(key, null);
            else
                SetValue<object>(key, valueToken.ToObject<object>());
        }
    }

    // 글로벌 변수 디버깅/에디터 노출용
    public static IReadOnlyDictionary<string, object> GetGlobalData()
    {
        return globalData;
    }
}
