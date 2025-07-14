using System;
using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using UnityEngine;

public class ControllerManager : MonoBehaviour
{
    public static ControllerManager Instance;
    private List<Controller> controllers = new List<Controller>();

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
    }

    public void Reset()
    {
        foreach (var c in controllers)
            Destroy(c.gameObject);

        controllers.Clear();
    }

    public void Init(JArray controllersJArray)
    {
        Reset();

        foreach (JObject controllerJson in controllersJArray)
        {
            GameObject controllerGameObject = new GameObject("Controller");

            Type type = Type.GetType(controllerJson["type"].Value<string>());
            Controller controller = (Controller)controllerGameObject.AddComponent(type);
            controller.FromJson(controllerJson);
            controllers.Add(controller);
        }
    }

    public T GetControllerByIndex<T>(int index) where T : Controller
    {
        if (index < 0 || index >= controllers.Count) return null;

        Controller controller = controllers[index];

        if (controller == null) return null;

        return controller.GetComponent<T>();
    }

    public T GetControllerById<T>(string id) where T : Controller
    {
        Controller controller = controllers.Find(c => c.GetId() == id);

        if (controller == null) return null;

        return controller.GetComponent<T>();
    }
}
