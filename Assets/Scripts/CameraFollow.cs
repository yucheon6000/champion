using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public float smoothSpeed = 5f;
    public Vector3 offset = new Vector3(0, 0, -10);

    void LateUpdate()
    {
        if (CameraTarget.CurrentTarget != null)
        {
            Vector3 targetPosition = CameraTarget.CurrentTarget.transform.position + offset;
            transform.position = Vector3.Lerp(transform.position, targetPosition, Time.deltaTime * smoothSpeed);
        }
    }
}