using System;
using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using UnityEditor;
using UnityEngine;

public class Entity : MonoBehaviour
{
    [Header("Read Only!")]
    [SerializeField]
    private List<IComponent> components;
    private List<string> Tags = new List<string>();

    [Header("Stats UI")]
    [SerializeField]
    private RectTransform statsUIParent;
    private Transform StatsUICanvasTransform => statsUIParent.parent.transform;
    [SerializeField]
    private GameObject statUIPrefab;

    public JObject ToJson()
    {
        var json = new JObject();

        var componentsJArray = new JArray();
        foreach (IComponent component in components)
            componentsJArray.Add(component.ToJson());
        json.Add("components", componentsJArray);

        // Color
        var renderer = GetComponent<SpriteRenderer>();
        if (renderer != null)
            json["color"] = $"#{ColorUtility.ToHtmlStringRGB(renderer.color)}";

        return json;
    }

    public void FromJson(JObject json)
    {
        JArray componentsJArray = (JArray)json["components"];
        AddComponentByJson(componentsJArray);

        if (json.TryGetValue("tags", out var tagToken) && tagToken is JArray tagArray)
            foreach (var tag in tagArray)
                Tags.Add(tag.ToString());

        if (json.TryGetValue("color", out var colorToken))
        {
            if (ColorUtility.TryParseHtmlString(colorToken.Value<string>(), out var color))
            {
                SpriteRenderer renderer = GetComponent<SpriteRenderer>();
                if (renderer) renderer.color = color;
            }
        }

        InitStatsUI();
    }

    public bool HasTag(string tag) => Tags.Contains(tag);

    private void AddComponentByJson(JArray jsonArray)
    {
        List<IComponent> addedComponents = new List<IComponent>();

        foreach (JObject json in jsonArray)
        {
            string typeName = json["type"].Value<string>();
            Type type = Type.GetType(typeName);

            if (type == null) continue;

            IComponent component = (IComponent)gameObject.AddComponent(type);
            component.FromJson(json);

            components.Add(component);
            addedComponents.Add(component);
        }

        // Initialize all the added components.
        foreach (IComponent component in addedComponents)
            component.Init(this);
    }

    private void InitStatsUI()
    {
        Stats stats = GetIComponent<Stats>();
        if (stats == null) return;

        foreach (var k in stats.AllStats.Keys)
        {
            GameObject clone = Instantiate(statUIPrefab, statsUIParent);
            clone.GetComponent<StatUI>().Init(k, stats);
        }

        StatsUICanvasTransform.SetParent(null);
    }

    public T GetIComponent<T>() where T : IComponent
    {
        return GetComponent<T>();
    }

    public T[] GetIComponents<T>() where T : IComponent
    {
        return GetComponents<T>();
    }

    private void Update()
    {
        // 월드 위치 + 오프셋 → 스크린 위치로 변환
        Vector3 screenPos = Camera.main.WorldToScreenPoint(transform.position);

        // UI 위치 갱신
        statsUIParent.position = screenPos;
    }

    private void OnDestroy()
    {
        Destroy(StatsUICanvasTransform.gameObject);
    }
}
