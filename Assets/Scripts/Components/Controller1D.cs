using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using UnityEngine;

public class Controller1D : Controller
{
    public override void FromJson(JObject json)
    {
    }

    protected override string GetComponentType() => "Controller1D";
}
