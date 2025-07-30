using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;
using TMPro;
using UnityEngine;
using System.IO;
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
    private WinLoseManager winLoseManager;
    [SerializeField]
    private PromptManager promptManager;

    [SerializeField]
    private string fileName;

    [SerializeField]
    TMP_InputField jsonInputField;

    private string GetProjectRootPath()
    {
#if UNITY_EDITOR
        // 에디터에서는 Assets의 상위 폴더가 프로젝트 루트
        return Path.GetFullPath(Path.Combine(Application.dataPath, ".."));
#else
        // 빌드된 플레이어에서는 실행 파일 위치 기준
        string exePath = Application.dataPath;
        if (Application.platform == RuntimePlatform.WindowsPlayer)
            return Path.GetFullPath(Path.Combine(exePath, ".."));
        else if (Application.platform == RuntimePlatform.OSXPlayer)
            return Path.GetFullPath(Path.Combine(exePath, "../../"));
        else
            return Application.persistentDataPath; // 기타 플랫폼은 안전한 저장소 사용
#endif
    }

    private string lastJsonPath => Path.Combine(GetProjectRootPath(), "Data", fileName);

    JObject lastJson;

    void Start()
    {
        promptManager.Init();
        StopGame();
        promptManager.OnGetNewPrompt.AddListener(OnGetNewJson);
        promptManager.AddLastJson(lastJson.ToString());
    }

    private void InitFromLastJsonFile()
    {
        // 프로젝트 루트(Assets와 같은 레벨)에 json 파일이 있으면 불러와서 적용
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
        // 글로벌 변수 초기화
        JObject globalVariablesObj = (JObject)json["globalVariables"];
        Blackboard.SetGlobalFromJson(globalVariablesObj);

        // json 파일 저장
        File.WriteAllText(lastJsonPath, json.ToString());

        // Set controllers.
        JArray controllersJArray = (JArray)json["controllers"];
        controllerManager.Init(controllersJArray);

        // Set Preset.
        JObject presetsJObject = (JObject)json["presets"];
        presetManager.Init(presetsJObject);

        /*
        // Set camera (optional)
        if (json.TryGetValue("camera", out JToken cameraToken))
        {
            cameraManager.Init((JObject)cameraToken);
        }
        */

        // Set entities.
        JArray scenesJArray = (JArray)json["scenes"];
        foreach (JObject sceneJson in scenesJArray)
        {
            JArray entitiesJArray = (JArray)sceneJson["entities"];
            foreach (JObject entityJson in entitiesJArray)
            {
                string presetId = entityJson["presetId"].Value<string>();
                JArray positionJArray = (JArray)entityJson["position"];
                Vector2 position = new Vector2(positionJArray[0].Value<float>(), positionJArray[1].Value<float>());

                entityManager.CreateEntityFromPreset(presetId, position);
            }
        }

        // catch (System.Exception ex)
        // {
        //     Debug.LogWarning($"Failed to create game from Json: {ex.Message}");
        // }
    }
}
