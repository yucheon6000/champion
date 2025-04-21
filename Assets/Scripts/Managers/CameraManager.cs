using UnityEngine;
using Newtonsoft.Json.Linq;

public class CameraManager : MonoBehaviour
{
    [SerializeField]
    private Camera mainCamera;

    private Vector3 originalPosition;

    private void Awake()
    {
        originalPosition = mainCamera.transform.position;
    }

    public void Reset()
    {
        mainCamera.transform.position = originalPosition;
    }

    public void Init(JObject cameraJson)
    {
        if (mainCamera == null)
        {
            mainCamera = Camera.main;
        }

        if (mainCamera == null)
        {
            Debug.LogWarning("메인 카메라를 찾을 수 없습니다.");
            return;
        }

        // fov는 orthographicSize일 수도 있고 fieldOfView일 수도 있음
        if (cameraJson.TryGetValue("fov", out JToken fovToken))
        {
            float fovValue = fovToken.Value<float>();

            if (mainCamera.orthographic)
            {
                mainCamera.orthographicSize = fovValue;
            }
            else
            {
                mainCamera.fieldOfView = fovValue;
            }
        }
    }
}