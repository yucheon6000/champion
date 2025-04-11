using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using UnityEngine;
using UnityEngine.Events;

public class ControllerButton : Controller
{
    [SerializeField]
    private KeyCode keyCode = KeyCode.None;

    public UnityEvent OnGetKey { private set; get; } = new UnityEvent();
    public UnityEvent OnGetKeyDown { private set; get; } = new UnityEvent();
    public UnityEvent OnGetKeyUp { private set; get; } = new UnityEvent();


    private void Update()
    {
        if (keyCode == KeyCode.None) return;

        if (Input.GetKey(keyCode))
            OnGetKey.Invoke();
        if (Input.GetKeyDown(keyCode))
            OnGetKeyDown.Invoke();
        if (Input.GetKeyUp(keyCode))
            OnGetKeyUp.Invoke();
    }

    public override void FromJson(JObject json)
    {
        string keyCodeStr = json["keyCode"].Value<string>();
        keyCode = (KeyCode)System.Enum.Parse(typeof(KeyCode), keyCodeStr);
    }

    protected override string GetComponentType() => "ControllerButton";
}
