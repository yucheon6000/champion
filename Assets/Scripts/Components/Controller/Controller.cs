using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using UnityEngine;

public abstract class Controller : IComponent
{
    protected string id;
    public string GetId() => id;
}
