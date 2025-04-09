using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using UnityEngine;
using UnityEngine.Events;

public class Controller2D : Controller
{
    [SerializeField]
    public UnityEvent<Vector2> OnValueUpdated { private set; get; } = new UnityEvent<Vector2>();

    public override void FromJson(JObject json)
    {

    }

    protected override string GetComponentType() => "Controller2D";

    private void Update()
    {
        Vector2 dir = Vector2.zero;
        dir.x = Input.GetAxisRaw("Horizontal");
        dir.y = Input.GetAxisRaw("Vertical");

        OnValueUpdated.Invoke(dir.normalized);
    }
}
