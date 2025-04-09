using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using Newtonsoft.Json.Linq;
using UnityEngine;

public class Jumpable : ComponentUsingRigidbody2D
{
    [SerializeField]
    private float jumpForce;

    public void Jump()
    {
        CheckRigidbody2D();
        rigidbody2D.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
    }

    public override JObject ToJson()
    {
        var json = base.ToJson();
        json.Add("jumpForce", jumpForce);

        return json;
    }

    public override void FromJson(JObject json)
    {
        if (!IsCorrectJson(json)) return;

        jumpForce = json["jumpForce"].Value<float>();

        // Add listener to the controller
        int listenTo = json["listenTo"].Value<int>();
        ControllerButton controller = ControllerManager.Instance.GetControllerByIndex<ControllerButton>(listenTo);
        if (controller)
        {
            controller.OnGetKeyDown.RemoveListener(Jump);
            controller.OnGetKeyDown.AddListener(Jump);
        }
    }

    protected override string GetComponentType() => "Jumpable";
}
