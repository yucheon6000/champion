using System.Collections;
using System.Collections.Generic;
using AdvancedInputFieldPlugin;
using OpenAI.Chat;
using UnityEngine;
using UnityEngine.UI;
using System.IO;
using UnityEngine.Events;
using Newtonsoft.Json.Linq;
using System.Text.RegularExpressions; // 추가

public class PromptManager : MonoBehaviour
{
    [Header("UI")]
    [SerializeField]
    AdvancedInputField promptInputField;
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

    public void Reset()
    {
        messages.Clear();

        // 날짜+시간 기반 파일명 (예: PromptLog_20240422_153012.txt)
        string fileName = $"PromptLog_{System.DateTime.Now:yyyyMMdd_HHmmss}.txt";
        logFilePath = Path.Combine(Application.persistentDataPath, fileName);
        File.WriteAllText(logFilePath, $"[Prompt Log Created {System.DateTime.Now}]\n");

        string systemMessage = Resources.Load<TextAsset>(systemPromptFileName).text;
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
        string userMessage = promptInputField.Text;
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

        promptInputField.Text = "";
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
            // fallback: 새 파일 생성
            string fileName = $"PromptLog_{System.DateTime.Now:yyyyMMdd_HHmmss}.txt";
            logFilePath = Path.Combine(Application.persistentDataPath, fileName);
        }
        File.AppendAllText(logFilePath, $"{System.DateTime.Now:yyyy-MM-dd HH:mm:ss} {log}\n");
    }

    public static string StripCodeFences(string input)
    {
        if (string.IsNullOrEmpty(input))
            return input;

        // 1) 맨 앞의 ``` 또는 ```json + 개행 제거
        input = Regex.Replace(input, @"\A```(?:json)?\r?\n", "");

        // 2) 맨 뒤의 ``` 제거
        input = Regex.Replace(input, @"\r?\n```$", "");

        return input;
    }
}
