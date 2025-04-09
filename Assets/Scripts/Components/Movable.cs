using Newtonsoft.Json.Linq;
using UnityEngine;

public class Movable : ComponentUsingRigidbody2D
{
    [Header("Read Only!")]
    [SerializeField]
    private float moveSpeed;
    [SerializeField]
    private new Rigidbody2D rigidbody;

    public void Move(Vector2 direction)
    {
        transform.Translate(direction * moveSpeed * Time.deltaTime, Space.World);
    }

    public override JObject ToJson()
    {
        var json = base.ToJson();
        json.Add("moveSpeed", moveSpeed);
        return json;
    }

    public override void FromJson(JObject json)
    {
        if (!IsCorrectJson(json)) return;

        moveSpeed = json["moveSpeed"].Value<float>();

        // Add listener to the controller
        int listenTo = json["listenTo"].Value<int>();
        Controller2D controller = FindControllerByIndex<Controller2D>(listenTo);
        if (!controller) return;

        controller.OnValueUpdated.RemoveListener(Move);
        controller.OnValueUpdated.AddListener(Move);
    }

    protected override string GetComponentType() => "Movable";
}
