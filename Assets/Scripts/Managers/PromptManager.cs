using System.Collections;
using System.Collections.Generic;
using AdvancedInputFieldPlugin;
using UnityEngine;
using UnityEngine.UI;
using System.IO;
using UnityEngine.Events;
using Newtonsoft.Json.Linq;
using System.Text.RegularExpressions;
using TMPro;
using System.Threading.Tasks;
using GenerativeAI;
using GenerativeAI.Types;

public class PromptManager : MonoBehaviour
{
    [Header("UI")]
    [SerializeField]
    TMP_InputField promptInputField;
    [SerializeField]
    AdvancedInputField assistantInputField;
    [SerializeField]
    Button submitButton;

    [Header("AI")]
    [SerializeField]
    private string modelName = "gemini-1.5-pro";
    [SerializeField]
    private string systemPromptFileName = "SystemPrompt";

    GoogleAi googleAi;
    GenerativeModel model;
    List<Content> messages = new List<Content>();
    private string logFilePath;

    public UnityEvent<JObject> OnGetNewPrompt { private set; get; } = new UnityEvent<JObject>();

    private void Start()
    {
        print(NodeDocumentationGenerator.GenerateNodeDocumentation());
    }

    private string GetProjectRootPath()
    {
#if UNITY_EDITOR
        // In the editor, the project root is the parent of Assets
        return Path.GetFullPath(Path.Combine(Application.dataPath, ".."));
#else
        // In builds, use executable location
        string exePath = Application.dataPath;
        if (Application.platform == RuntimePlatform.WindowsPlayer)
            return Path.GetFullPath(Path.Combine(exePath, ".."));
        else if (Application.platform == RuntimePlatform.OSXPlayer)
            return Path.GetFullPath(Path.Combine(exePath, "../../"));
        else
            return Application.persistentDataPath; // Safe storage for other platforms
#endif
    }

    // 로그 파일을 프로젝트 루트의 Data 폴더에 저장
    private string GetLogFilePath(string fileName)
    {
        string dataFolder = Path.Combine(GetProjectRootPath(), "Data");
        if (!Directory.Exists(dataFolder))
        {
            Directory.CreateDirectory(dataFolder);
        }
        return Path.Combine(dataFolder, fileName);
    }

    public void Reset()
    {
        messages.Clear();

        // 날짜+시간 기반 파일명 (예: PromptLog_20240422_153012.txt)
        string fileName = $"PromptLog_{System.DateTime.Now:yyyyMMdd_HHmmss}.txt";
        logFilePath = GetLogFilePath(fileName);
        File.WriteAllText(logFilePath, $"[Prompt Log Created {System.DateTime.Now}]\n");

        string systemMessage = Resources.Load<TextAsset>(systemPromptFileName).text;
        systemMessage = systemMessage.Replace("{Node Document}", NodeDocumentationGenerator.GenerateNodeDocumentation());
        messages.Add(new Content { Role = "user", Parts = { new Part { Text = systemMessage } } });
        LogToFile($"[System]\n{systemMessage}\n");
    }

    public void Init()
    {
        string apiKey = Resources.Load("gemini-api-key").ToString();
        googleAi = new GoogleAi(apiKey);
        model = googleAi.CreateGenerativeModel(modelName);

        Reset();
    }

    public void SubmitPrompt()
    {
        if (!promptInputField.enabled) return;

        EnableUI(false);

        StartCoroutine(GetAnswer());
    }

    public void AddLastJson(string json)
    {
        messages.Add(new Content { Role = "model", Parts = { new Part { Text = json } } });
        LogToFile($"[Assistant]\n{json}\n");
    }

    private IEnumerator GetAnswer()
    {
        // 프롬프트 입력 필드의 텍스트를 사용자 메시지로 추가
        string userMessage = promptInputField.text;
        messages.Add(new Content { Role = "user", Parts = { new Part { Text = userMessage } } });
        LogToFile($"[User]\n{userMessage}\n");

        // 메시지를 전송하고 결과를 기다림
        var result = model.GenerateContentAsync(new GenerateContentRequest { Contents = messages });
        yield return new WaitUntil(() => result.IsCompleted);

        try
        {
            if (result.Exception != null)
            {
                Debug.LogError($"[PromptManager] API 호출 중 오류 발생: {result.Exception.Message}");
                assistantInputField.Text = $"오류가 발생했습니다: {result.Exception.Message}";
                EnableUI(true);
                yield break;
            }

            // 결과를 사용자 메시지로 추가
            string assistantMessage = result.Result.Text;
            messages.Add(new Content { Role = "model", Parts = { new Part { Text = assistantMessage } } });
            LogToFile($"[Assistant]\n{assistantMessage}\n");

            // 결과를 파싱하고 이벤트 호출
            JObject json = JObject.Parse(StripCodeFences(assistantMessage));
            OnGetNewPrompt.Invoke(json);

            // 결과를 입력 필드에 표시
            assistantInputField.Text = json["assistant"].Value<string>();
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"[PromptManager] 예외 발생: {ex.Message}");
            assistantInputField.Text = $"오류가 발생했습니다: {ex.Message}";
        }

        // 입력 필드 초기화
        promptInputField.text = "";
        EnableUI(true);
    }

    private void EnableUI(bool value)
    {
        promptInputField.enabled = value;
        submitButton.enabled = value;
    }

    private void LogToFile(string log)
    {
        if (string.IsNullOrEmpty(logFilePath))
        {
            string fileName = $"PromptLog_{System.DateTime.Now:yyyyMMdd_HHmms}.txt";
            logFilePath = GetLogFilePath(fileName);
        }
        File.AppendAllText(logFilePath, $"{System.DateTime.Now:yyyy-MM-dd HH:mm:ss} {log}\n");
    }

    public static string StripCodeFences(string input)
    {
        if (string.IsNullOrEmpty(input))
            return input;

        // ``` 또는 ```json 으로 감싸진 부분만 추출
        var match = Regex.Match(input, @"```(?:json)?\r?\n([\s\S]*?)\r?\n```", RegexOptions.Multiline);
        if (match.Success)
        {
            return match.Groups[1].Value;
        }
        else
        {
            // ```가 없으면 원본 반환
            return input;
        }
    }
}
