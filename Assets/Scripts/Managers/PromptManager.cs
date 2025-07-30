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
using GenerativeAI.Clients;
using System.Linq;

public class PromptManager : MonoBehaviour
{
    public static PromptManager Instance { private set; get; }

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

    void Awake()
    {
        if (Instance == null)
            Instance = this;

        StartCoroutine(GenerateSpriteRoutine());
    }

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

        InitSpriteCache();
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
            if (json.TryGetValue("assistant", out var assistantToken))
                assistantInputField.Text = assistantToken.Value<string>();
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

    // ===========================================
    // 이미지 생성 관련 메서드들
    // ===========================================

    private ImagenModel imagenModel;

    private string spritesFolder => Path.Combine(GetProjectRootPath(), "Assets", "Resources", "Sprites");
    private Dictionary<string, Sprite> spriteCache = new Dictionary<string, Sprite>();
    struct SpriteRequest
    {
        public string presetId;
        public string prompt;
        public bool hasTransparent;
    }
    private Queue<SpriteRequest> spriteRequestQueue = new Queue<SpriteRequest>();
    private bool isGeneratingSprite = false;

    private void InitSpriteCache()
    {
        string[] spriteFiles = Directory.GetFiles(spritesFolder, "*.png");
        Debug.Log($"[PromptManager] 스프라이트 캐시 초기화: {spriteFiles.Length}개");
        foreach (string spriteFile in spriteFiles)
        {
            try
            {
                string spriteName = Path.GetFileNameWithoutExtension(spriteFile);

                // 파일에서 직접 텍스처 로드
                byte[] fileData = File.ReadAllBytes(spriteFile);
                Texture2D texture = new Texture2D(1, 1);

                if (texture.LoadImage(fileData))
                {
                    // 가장 긴 축을 1로 정규화하기 위한 pixelsPerUnit 계산
                    float pixelsPerUnit = Mathf.Max(texture.width, texture.height);
                    Sprite sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f), pixelsPerUnit);
                    spriteCache[spriteName] = sprite;
                    Debug.Log($"[PromptManager] 스프라이트 캐시 추가: {spriteName} ({texture.width}x{texture.height}, PPU: {pixelsPerUnit})");
                }
                else
                {
                    Debug.LogError($"[PromptManager] 스프라이트 로드 실패: {spriteName}");
                }
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"[PromptManager] 스프라이트 로드 중 오류: {Path.GetFileName(spriteFile)} - {ex.Message}");
            }
        }
    }

    public bool HasSprite(string presetId)
    {
        return spriteCache.ContainsKey(presetId);
    }

    public Sprite GetSprite(string presetId)
    {
        return spriteCache[presetId];
    }

    /// <summary>
    /// 텍스트 프롬프트를 기반으로 스프라이트 이미지를 생성하고 파일로 저장합니다.
    /// </summary>
    /// <param name="presetId">저장할 파일명 (확장자 제외)</param>
    /// <param name="prompt">이미지 생성을 위한 텍스트 프롬프트</param>
    public void GenerateSprite(string presetId, string prompt, bool hasTransparent = true)
    {
        if (HasSprite(presetId))
        {
            Debug.Log($"[PromptManager] 스프라이트가 이미 있습니다: {presetId}");
            return;
        }

        if (spriteRequestQueue.Any(request => request.presetId == presetId))
        {
            Debug.Log($"[PromptManager] 스프라이트 생성 요청이 이미 있습니다: {presetId}");
            return;
        }

        // 이미 생성 중이면 큐에만 추가
        spriteRequestQueue.Enqueue(new SpriteRequest { presetId = presetId, prompt = prompt, hasTransparent = hasTransparent });
        Debug.Log($"[PromptManager] 스프라이트 생성 요청 추가: {presetId} (큐 크기: {spriteRequestQueue.Count})");
    }

    private IEnumerator GenerateSpriteRoutine()
    {
        WaitForSeconds wait = new WaitForSeconds(1f);

        // Imagen 모델 초기화 (한 번만)
        if (imagenModel == null)
        {
            string apiKey = Resources.Load<TextAsset>("billing-gemini-api-key").text;
            GoogleAi googleAi = new GoogleAi(apiKey);
            imagenModel = googleAi.CreateImageModel("imagen-3.0-generate-002");
            Debug.Log("[PromptManager] Imagen 모델 초기화 완료");

            // 모델 초기화 후 잠시 대기
            yield return wait;
        }

        while (true)
        {
            if (spriteRequestQueue.Count == 0)
            {
                yield return wait;
                continue;
            }

            SpriteRequest request = spriteRequestQueue.Dequeue();

            Debug.Log($"[PromptManager] 이미지 생성 시작: {request.presetId} - {request.prompt}");

            // 이미지 생성 요청 설정
            GenerateImageRequest imageRequest = new GenerateImageRequest();

            string prompt = request.prompt;
            if (request.hasTransparent)
                prompt += "The background must be a solid bright green color. Do not use any green colors for the main object or other elements in the image. This is important for creating images with transparent backgrounds.";
            else
                prompt += "The object should fill the entire frame without any background or empty space. Draw only the object itself, nothing else.";

            imageRequest.AddPrompt(prompt);

            imageRequest.AddParameters(new ImageGenerationParameters()
            {
                SampleCount = 1,
                AspectRatio = "1:1"
            });

            // API 호출 전 대기 (Rate limiting 방지)
            yield return new WaitForSeconds(2f);

            // API 호출 및 대기
            var response = imagenModel.GenerateImagesAsync(imageRequest);
            yield return new WaitUntil(() => response.IsCompleted);

            // 에러 처리
            if (response.Exception != null)
            {
                Debug.LogError($"[PromptManager] 이미지 생성 실패: {response.Exception.Message}");

                // "More than one instance" 오류인 경우 더 오래 대기
                if (response.Exception.Message.Contains("More than one instance"))
                {
                    Debug.LogWarning("[PromptManager] API 인스턴스 충돌 감지, 5초 대기 후 재시도");
                    yield return new WaitForSeconds(5f);
                }

                spriteRequestQueue.Enqueue(request);
                yield return wait;
                continue;
            }

            if (response.Result?.Predictions == null || response.Result.Predictions.Count == 0)
            {
                Debug.LogError("[PromptManager] 생성된 이미지가 없습니다.");
                spriteRequestQueue.Enqueue(request);
                yield return wait;
                continue;
            }

            // Base64 → Texture2D 변환 및 후처리
            string base64String = response.Result.Predictions[0].BytesBase64Encoded;

            if (string.IsNullOrEmpty(base64String))
            {
                Debug.LogError("[PromptManager] Base64 데이터가 비어있습니다.");
                spriteRequestQueue.Enqueue(request);
                yield return wait;
                continue;
            }

            Texture2D processedTexture = Base64ToTexture2D(base64String);

            if (request.hasTransparent)
                processedTexture = ProcessImageData(processedTexture);

            if (processedTexture != null)
            {
                string spriteName = request.presetId;

                try
                {
                    // 스프라이트 캐시에 저장 - 가장 긴 축을 1로 정규화
                    float pixelsPerUnit = Mathf.Max(processedTexture.width, processedTexture.height);
                    spriteCache[spriteName] = Sprite.Create(processedTexture, new Rect(0, 0, processedTexture.width, processedTexture.height), new Vector2(0.5f, 0.5f), pixelsPerUnit);

                    // 파일로 저장
                    SaveTextureToFile(processedTexture, spriteName);

                    Debug.Log($"[PromptManager] 스프라이트 캐시 및 파일 저장 완료: {spriteName} ({processedTexture.width}x{processedTexture.height}, PPU: {pixelsPerUnit})");
                }
                catch (System.Exception ex)
                {
                    Debug.LogError($"[PromptManager] 스프라이트 저장 실패: {ex.Message}");
                    spriteRequestQueue.Enqueue(request);
                    continue;
                }

                yield return wait;
                continue;
            }
            else
            {
                Debug.LogError("[PromptManager] 이미지 처리 실패 - processedTexture가 null입니다.");

                spriteRequestQueue.Enqueue(request);
                yield return wait;
                continue;
            }
        }



    }

    private Texture2D Base64ToTexture2D(string base64String)
    {
        try
        {
            byte[] imageBytes = System.Convert.FromBase64String(base64String);
            Texture2D texture = new Texture2D(1, 1);

            if (texture.LoadImage(imageBytes))
            {
                Debug.Log($"[PromptManager] Base64 변환 성공: {texture.width}x{texture.height}");
                return texture;
            }
            else
            {
                Debug.LogError("[PromptManager] LoadImage 실패");
                return null;
            }
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"[PromptManager] Base64 변환 실패: {ex.Message}");
            return null;
        }
    }

    /// <summary>
    /// Base64 이미지 데이터를 처리하여 크로마키 및 정규화가 적용된 Texture2D를 반환합니다.
    /// </summary>
    private Texture2D ProcessImageData(Texture2D texture)
    {
        try
        {
            // 후처리 적용
            texture = ApplyChromaKey(texture);
            texture = CreateNormalizedTexture(texture);

            return texture;
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"[PromptManager] 이미지 처리 중 오류: {ex.Message}");
            return null;
        }
    }

    /// <summary>
    /// 처리된 텍스처를 PNG 파일로 저장합니다.
    /// </summary>
    private void SaveTextureToFile(Texture2D texture, string spriteName)
    {
        try
        {
            // 저장 폴더 경로 생성
            if (!Directory.Exists(spritesFolder))
            {
                Directory.CreateDirectory(spritesFolder);
                Debug.Log($"[PromptManager] 폴더 생성: {spritesFolder}");
            }

            // 파일 저장
            string filePath = Path.Combine(spritesFolder, spriteName + ".png");
            byte[] pngBytes = texture.EncodeToPNG();
            File.WriteAllBytes(filePath, pngBytes);

            Debug.Log($"[PromptManager] 파일 저장 완료: {filePath}");
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"[PromptManager] 파일 저장 실패: {ex.Message}");
        }
    }

    /// <summary>
    /// 초록색 배경을 투명하게 변환합니다 (크로마키 효과).
    /// </summary>
    private Texture2D ApplyChromaKey(Texture2D originalTexture)
    {
        Texture2D newTexture = new Texture2D(originalTexture.width, originalTexture.height, TextureFormat.RGBA32, false);
        Color[] pixels = originalTexture.GetPixels();

        for (int i = 0; i < pixels.Length; i++)
        {
            Color pixel = pixels[i];

            // HSV 기준 초록색 감지
            Color.RGBToHSV(pixel, out float h, out float s, out float v);

            bool isGreen = false;

            // 초록색 범위: Hue 0.25~0.45, Saturation > 0.3, Value > 0.3
            if (h >= 0.25f && h <= 0.45f && s > 0.3f && v > 0.3f)
            {
                isGreen = true;
            }
            // RGB 기준 밝은 초록색
            else if (pixel.g > 0.6f && pixel.g > pixel.r * 1.5f && pixel.g > pixel.b * 1.5f)
            {
                isGreen = true;
            }

            pixels[i] = isGreen ? Color.clear : pixel;
        }

        newTexture.SetPixels(pixels);
        newTexture.Apply();

        return newTexture;
    }

    /// <summary>
    /// 투명하지 않은 영역을 기준으로 텍스처 크기를 정규화합니다.
    /// </summary>
    private Texture2D CreateNormalizedTexture(Texture2D texture)
    {
        // 실제 캐릭터 영역 계산
        Rect bounds = GetNonTransparentBounds(texture);

        if (bounds.width == 0 || bounds.height == 0)
        {
            Debug.LogWarning("[PromptManager] 투명하지 않은 영역을 찾을 수 없음, 전체 텍스처 사용");
            return texture;
        }

        // 영역 추출하여 새 텍스처 생성
        Texture2D normalizedTexture = new Texture2D((int)bounds.width, (int)bounds.height);
        normalizedTexture.SetPixels(texture.GetPixels((int)bounds.x, (int)bounds.y, (int)bounds.width, (int)bounds.height));
        normalizedTexture.Apply();

        Debug.Log($"[PromptManager] 텍스처 정규화 완료: {bounds.width}x{bounds.height}");

        return normalizedTexture;
    }

    /// <summary>
    /// 투명하지 않은 픽셀들의 경계 영역을 계산합니다.
    /// </summary>
    private Rect GetNonTransparentBounds(Texture2D texture)
    {
        Color[] pixels = texture.GetPixels();
        int width = texture.width;
        int height = texture.height;

        int minX = width, maxX = 0;
        int minY = height, maxY = 0;

        // 투명하지 않은 픽셀의 최소/최대 좌표 찾기
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                Color pixel = pixels[y * width + x];

                if (pixel.a > 0.1f) // 거의 투명하지 않은 픽셀
                {
                    minX = Mathf.Min(minX, x);
                    maxX = Mathf.Max(maxX, x);
                    minY = Mathf.Min(minY, y);
                    maxY = Mathf.Max(maxY, y);
                }
            }
        }

        // 유효한 경계 반환
        if (minX < maxX && minY < maxY)
        {
            return new Rect(minX, minY, maxX - minX + 1, maxY - minY + 1);
        }

        return new Rect(0, 0, 0, 0); // 투명한 픽셀만 있는 경우
    }
}
