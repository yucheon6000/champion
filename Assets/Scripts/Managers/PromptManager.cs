using System.Collections;
using System.Collections.Generic;
using AdvancedInputFieldPlugin;
using OpenAI.Chat;
using UnityEngine;
using UnityEngine.UI;
using System.IO;
using UnityEngine.Events;
using Newtonsoft.Json.Linq;
using System.Text.RegularExpressions;
using TMPro; // 추가

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
    private string model = "gpt-4.1-mini";
    [SerializeField]
    private string systemPromptFileName = "SystemPrompt";

    ChatClient client;
    List<ChatMessage> messages = new List<ChatMessage>();
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
        messages.Add(new SystemChatMessage(systemMessage));
        LogToFile($"[System]\n{systemMessage}\n");
    }

    public void Init()
    {
        client = new(model: model, apiKey: Resources.Load("openai-api-key").ToString());

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
        messages.Add(new AssistantChatMessage(json));
        LogToFile($"[Assistant]\n{json}\n");
    }

    private IEnumerator GetAnswer()
    {
        string userMessage = promptInputField.text;
        messages.Add(new UserChatMessage(userMessage));
        LogToFile($"[User]\n{userMessage}\n");

        var result = client.CompleteChatAsync(messages);
        yield return new WaitUntil(() => result.IsCompleted);

        string assistantMessage = result.Result.Value.Content[0].Text;
        messages.Add(new AssistantChatMessage(assistantMessage));
        LogToFile($"[Assistant]\n{assistantMessage}\n");

        JObject json = JObject.Parse(StripCodeFences(assistantMessage));
        OnGetNewPrompt.Invoke(json);

        assistantInputField.Text = json["assistant"].Value<string>();

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
