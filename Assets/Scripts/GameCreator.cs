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

    [Header("Prefabs")]
    [SerializeField]
    private GameObject entityPrefab;

    [SerializeField]
    private string fileName;

    [SerializeField]
    Entity entity;

    [SerializeField]
    TMP_InputField jsonInputField;

    void Start()
    {
        Init();
    }

    private void Init()
    {
        // Resources 폴더에서 json 파일 불러오기 (확장자 생략)
        TextAsset jsonFile = Resources.Load<TextAsset>(fileName);
        jsonInputField.text = jsonFile.text;

        if (jsonFile != null)
        {
            JObject json = JObject.Parse(jsonFile.text);

            // Set controllers.
            JArray controllersJArray = (JArray)json["controllers"];
            controllerManager.Init(controllersJArray);

            // Set entities.
            JArray entitiesJArray = (JArray)json["entities"];
            foreach (JObject entityJson in entitiesJArray)
            {
                var posRaw = entityJson["position"].Values<float>().ToArray();
                Vector2 pos = new Vector2(posRaw[0], posRaw[1]);
                Entity entity = Instantiate(entityPrefab, pos, Quaternion.identity).GetComponent<Entity>();
                entity.FromJson(entityJson);
            }
        }
        else
        {
            Debug.LogWarning("JSON 파일을 찾을 수 없습니다.");
        }
    }
}
