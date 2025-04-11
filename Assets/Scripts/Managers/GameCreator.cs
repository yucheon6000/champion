using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;
using TMPro;
using UnityEngine;

public class GameCreator : MonoBehaviour
{
    [Header("Managers")]
    [SerializeField]
    private ControllerManager controllerManager;
    [SerializeField]
    private PresetManager presetManager;
    [SerializeField]
    private EntityManager entityManager;
    [SerializeField]
    private CameraManager cameraManager;

    [SerializeField]
    private string fileName;

    [SerializeField]
    TMP_InputField jsonInputField;

    void Start()
    {
        Init();
    }

    private void Init()
    {
        TextAsset jsonFile = Resources.Load<TextAsset>(fileName);
        jsonInputField.text = jsonFile.text;

        if (jsonFile == null)
        {
            Debug.LogWarning("JSON 파일을 찾을 수 없습니다.");
            return;
        }

        JObject json = JObject.Parse(jsonFile.text);

        // Set controllers.
        JArray controllersJArray = (JArray)json["controllers"];
        controllerManager.Init(controllersJArray);

        // Set Preset.
        JObject presetsJObject = (JObject)json["presets"];
        presetManager.Init(presetsJObject);

        // Set camera (optional)
        if (json.TryGetValue("camera", out JToken cameraToken))
        {
            cameraManager.Init((JObject)cameraToken);
        }

        // Set entities.
        JArray entitiesJArray = (JArray)json["entities"];
        foreach (JObject entityJson in entitiesJArray)
        {
            entityManager.CreateEntityFromJson(entityJson);
        }
    }
}
