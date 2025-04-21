using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;
using TMPro;
using UnityEngine;
using System.IO;

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
    private WinLoseManager winLoseManager;
    [SerializeField]
    private PromptManager promptManager;

    [SerializeField]
    private string fileName;

    [SerializeField]
    TMP_InputField jsonInputField;

    private string lastJsonPath => Path.Combine(Application.persistentDataPath, fileName);

    JObject lastJson;

    void Start()
    {
        StopGame();
        promptManager.Init();
        promptManager.OnGetNewPrompt.AddListener(OnGetNewJson);
        promptManager.AddLastJson(lastJson.ToString());
    }

    private void InitFromLastJsonFile()
    {
        // 마지막 json 파일이 있으면 불러와서 적용
        if (File.Exists(lastJsonPath))
        {
            string jsonText = File.ReadAllText(lastJsonPath);
            lastJson = JObject.Parse(jsonText);
            InitFromJson(lastJson);
        }
    }

    public void PlayGame()
    {
        Time.timeScale = 1;
    }

    public void ResetGame()
    {
        StopGame();
        ResetAllManagers();
        promptManager.Reset();
    }

    private void ResetAllManagers()
    {
        controllerManager.Reset();
        presetManager.Reset();
        cameraManager.Reset();
        entityManager.ClearAllEntities();
        winLoseManager.Reset();
    }

    public void StopGame()
    {
        ResetAllManagers();
        InitFromLastJsonFile();
        Time.timeScale = 0;
    }

    public void OnGetNewJson(JObject json)
    {
        ResetAllManagers();
        Time.timeScale = 0;
        InitFromJson(json);
    }

    private void InitFromJson(JObject json)
    {
        // json 파일 저장
        File.WriteAllText(lastJsonPath, json.ToString());

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
