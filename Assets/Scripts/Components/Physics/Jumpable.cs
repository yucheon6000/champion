using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using Newtonsoft.Json.Linq;
using UnityEngine;

public class Jumpable : ComponentUsingRigidbody2D
{
    [SerializeField]
    private float jumpForce = 0;
    [SerializeField]
    private int listenTo = -1; // 기본값 설정

    public void Jump()
    {
        CheckRigidbody2D();
        rigidbody2D.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
    }

    public override JObject ToJson()
    {
        var json = base.ToJson();

        json.Add("jumpForce", jumpForce);
        json.Add("listenTo", listenTo); // listenTo 값 추가
        return json;
    }

    public override void FromJson(JObject json)
    {
        if (json.TryGetValue("jumpForce", out var forceToken))
            jumpForce = forceToken.Value<float>();

        if (json.TryGetValue("listenTo", out var listenToToken))
        {
            listenTo = listenToToken.Value<int>(); // listenTo 값을 변수에 저장
            ControllerButton controller = FindControllerByIndex<ControllerButton>(listenTo);
            if (controller)
            {
                controller.OnGetKeyDown.RemoveListener(Jump);
                controller.OnGetKeyDown.AddListener(Jump);
            }
        }
    }

    protected override string GetComponentType() => "Jumpable";
}
