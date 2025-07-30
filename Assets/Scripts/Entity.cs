using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;
using UnityEditor;
using UnityEngine;

public class Entity : MonoBehaviour
{
    private string presetId;

    [Header("Read Only!")]
    [SerializeField]
    private List<string> tags = new List<string>();
    public List<string> Tags => tags;

    // [SerializeField]
    // private List<IComponent> components;
    [SerializeField]
    private SpriteRenderer spriteRenderer;
    private bool hasSprite = false;

    [Header("Stats UI")]
    [SerializeField]
    private RectTransform statsUIParent;
    private Transform StatsUICanvasTransform => statsUIParent.parent.transform;
    [SerializeField]
    private GameObject statUIPrefab;

    private Blackboard blackboard = new Blackboard();
    public Blackboard Blackboard => blackboard;

    private BehaviorTreeRunner behaviorTreeRunner;
    public BehaviorTreeRunner BehaviorTreeRunner => behaviorTreeRunner;

    private bool willBeDestroyed = false;
    public bool WillBeDestroyed => willBeDestroyed;
    private int destroyCallTime = -1;

    public JObject ToJson()
    {
        var json = new JObject();

        // var componentsJArray = new JArray();
        // foreach (IComponent component in components)
        //     componentsJArray.Add(component.ToJson());
        // json.Add("components", componentsJArray);

        // Color
        var renderer = GetComponent<SpriteRenderer>();
        if (renderer != null)
            json["color"] = $"#{ColorUtility.ToHtmlStringRGB(renderer.color)}";

        return json;
    }

    public void FromJson(JObject json)
    {
        // Set variables to blackboard
        blackboard = new Blackboard();

        // Set tags
        if (json.TryGetValue("tags", out var tagToken) && tagToken is JArray tagArray)
            foreach (var tag in tagArray)
                Tags.Add(tag.ToString());

        // Set color
        if (json.TryGetValue("color", out var colorToken))
        {
            if (ColorUtility.TryParseHtmlString(colorToken.Value<string>(), out var color))
            {
                SpriteRenderer renderer = GetComponent<SpriteRenderer>();
                if (renderer) renderer.color = color;
            }
        }

        // Set variables to blackboard
        if (json.TryGetValue("variables", out JToken variablesToken) && variablesToken is JObject variablesObj)
        {
            blackboard.SetFromJson(variablesObj);
        }

        // Set a root node for a BT runner.
        if (json.TryGetValue("behaviorTree", out JToken rootNodeToken))
        {
            JObject rootNodeJson = rootNodeToken as JObject;
            if (rootNodeJson != null && rootNodeJson.Count > 0)
            {
                Node rootNode = BehaviorTreeFactory.CreateNodeFromJson(rootNodeJson);

                if (rootNode == null)
                {
                    Debug.LogWarning($"Failed to create root node from JSON: {rootNodeJson}");
                }
                else
                {
                    // Create a root node.
                    rootNode.FromJson(rootNodeJson);

                    // Add components to the entity.
                    RequiresBTComponentAttribute[] allRequiredAttrs = rootNode.GetRequiredBTComponents();

                    var uniqueComponentTypes = allRequiredAttrs.Select(attr => attr.RequiredComponentType).Distinct();
                    foreach (var type in uniqueComponentTypes)
                    {
                        if (GetComponent(type) == null)
                            gameObject.AddComponent(type);
                    }

                    // Create a behavior tree runner.
                    behaviorTreeRunner = new BehaviorTreeRunner(this, rootNode);
                }
            }
        }

        // Initialize all components
        InitAllComponents();

        // Initialize stat UI
        InitStatsUI();
    }

    public bool HasTag(string tag) => Tags.Contains(tag);

    private void InitAllComponents()
    {
        if (HasTag("Fixed"))
            GetComponent<Rigidbody2D>().constraints = RigidbodyConstraints2D.FreezeAll;

        if (HasTag("Gravity"))
            gameObject.AddComponent<Gravity>();

        if (HasTag("CameraTarget"))
            gameObject.AddComponent<CameraTarget>().Init(this);
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
        InitSpriteRenderer();

        // 월드 위치 + 오프셋 → 스크린 위치로 변환
        Vector3 screenPos = Camera.main.WorldToScreenPoint(transform.position);

        // UI 위치 갱신
        statsUIParent.position = screenPos;

        BehaviorTreeRunner?.Execute();

        // After 1 frame, destroy the entity.
        if (willBeDestroyed && Time.frameCount == destroyCallTime + 1)
        {
            Destroy(gameObject);
        }
    }

    public void SetPresetId(string presetId)
    {
        this.presetId = presetId;
    }

    private void InitSpriteRenderer()
    {
        if (hasSprite) return;

        if (spriteRenderer == null)
            spriteRenderer = GetComponent<SpriteRenderer>();

        if (spriteRenderer == null) return;

        if (PromptManager.Instance == null) return;

        if (!PromptManager.Instance.HasSprite(presetId)) return;

        Sprite sprite = PromptManager.Instance.GetSprite(presetId);
        if (sprite == null) return;

        spriteRenderer.sprite = sprite;
        spriteRenderer.color = Color.white;

        hasSprite = true;
    }

    public void Destroy()
    {
        willBeDestroyed = true;
        destroyCallTime = Time.frameCount;
    }

    private void OnDestroy()
    {
        Destroy(StatsUICanvasTransform.gameObject);
    }
}
