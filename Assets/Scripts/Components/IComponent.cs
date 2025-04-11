using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using UnityEngine;

public abstract class IComponent : MonoBehaviour
{
    protected Entity entity { private set; get; }

    public virtual void Init(Entity entity)
    {
        this.entity = entity;
    }

    public virtual JObject ToJson()
    {
        var json = new JObject
        {
            { "type", GetComponentType() }
        };
        return json;
    }

    public abstract void FromJson(JObject json);

    protected abstract string GetComponentType();

    protected bool IsCorrectJson(JObject json)
    {
        if (json.ContainsKey("type") == false) return false;

        return json["type"].Value<string>() == GetComponentType();
    }

    protected T FindControllerByIndex<T>(int index) where T : Controller
    {
        return ControllerManager.Instance.GetControllerByIndex<T>(index);
    }

    protected void Destroy(Entity entity)
    {
        EntityManager.Instance.DestroyEntity(entity);
    }

    protected Entity ClonePreset(string presetId, Vector2 position)
    {
        return EntityManager.Instance.CreateEntityFromPreset(presetId, position);
    }

    public T GetIComponent<T>() where T : IComponent
    {
        return entity.GetIComponent<T>();
    }

    public T[] GetIComponents<T>() where T : IComponent
    {
        return entity.GetIComponents<T>();
    }
}
