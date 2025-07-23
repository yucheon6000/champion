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

    public bool IsButtonDown { private set; get; } = false;
    public bool IsButtonHeld { private set; get; } = false;
    public bool IsButtonUp { private set; get; } = false;

    private void Update()
    {
        if (keyCode == KeyCode.None) return;

        IsButtonDown = false;
        IsButtonHeld = false;
        IsButtonUp = false;

        if (Input.GetKey(keyCode))
        {
            IsButtonHeld = true;
            OnGetKey.Invoke();
        }
        if (Input.GetKeyDown(keyCode))
        {
            IsButtonDown = true;
            OnGetKeyDown.Invoke();
        }
        if (Input.GetKeyUp(keyCode))
        {
            IsButtonUp = true;
            OnGetKeyUp.Invoke();
        }
    }

    public override void FromJson(JObject json)
    {
        base.FromJson(json);

        string keyCodeStr = json["keyCode"].Value<string>();
        keyCode = (KeyCode)System.Enum.Parse(typeof(KeyCode), keyCodeStr);
    }

    protected override string GetComponentType() => "ControllerButton";
}
