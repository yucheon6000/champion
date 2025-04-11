using Newtonsoft.Json.Linq;
using UnityEngine;

public class Movable : ComponentUsingRigidbody2D
{
    [Header("Read Only!")]
    [SerializeField]
    private float moveSpeed = 0;
    [SerializeField]
    private int listenTo = -1; // 기본값 설정

    public Vector2 MoveDirection { private set; get; } = Vector2.zero;

    public void Move(Vector2 direction)
        => Move(direction, Space.World);

    public void Move(Vector2 direction, Space space)
    {
        direction.Normalize();
        MoveDirection = direction;
        transform.Translate(direction * moveSpeed * Time.deltaTime, space);
    }

    public override JObject ToJson()
    {
        var json = base.ToJson();
        json.Add("moveSpeed", moveSpeed);
        json.Add("listenTo", listenTo); // listenTo 값 추가
        return json;
    }

    public override void FromJson(JObject json)
    {
        if (json.TryGetValue("moveSpeed", out var speedToken))
            moveSpeed = speedToken.Value<float>();

        if (json.TryGetValue("listenTo", out var listenToToken))
        {
            listenTo = listenToToken.Value<int>(); // listenTo 값을 변수에 저장
            Controller2D controller = FindControllerByIndex<Controller2D>(listenTo);
            if (controller)
            {
                controller.OnValueUpdated.RemoveListener(Move);
                controller.OnValueUpdated.AddListener(Move);
            }
        }
    }

    protected override string GetComponentType() => "Movable";
}
