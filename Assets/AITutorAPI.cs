using System.Collections;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

public class AITutorAPI : MonoBehaviour
{
    [SerializeField] private AITutorManager aiTutorManager;

    [Header("Backend")]
    [SerializeField]
    private string backendUrl = "http://192.168.100.70:8000";

    [System.Serializable]
    private class CutRequest
    {
        public string task;
        public float ratio_a;
        public float ratio_b;
        public float difference;
        public int cut_count;
    }

    [System.Serializable]
    private class AIResponse
    {
        public string reply;
    }

    // =========================================================
    // 啟動時自動測試 Quest → 電腦 Backend
    // =========================================================
    private void Start()
    {
        StartCoroutine(TestBackendConnection());
    }

    private IEnumerator TestBackendConnection()
    {
        string url = backendUrl;

        Debug.Log(
            "開始測試 AI Backend：" + url
        );

        UnityWebRequest request =
            UnityWebRequest.Get(url);

        // 不要讓測試卡很久
        request.timeout = 10;

        yield return request.SendWebRequest();

        if (request.result ==
            UnityWebRequest.Result.Success)
        {
            Debug.Log(
                "AI Backend 連線成功：" +
                request.downloadHandler.text
            );

            if (aiTutorManager != null)
            {
                aiTutorManager.ShowAnalysisFeedback(
                    "AI 後端連線成功！\n\n" +
                    "可以開始進行切割。"
                );
            }
        }
        else
        {
            Debug.LogError(
                "AI Backend 連線失敗：" +
                request.error
            );

            if (aiTutorManager != null)
            {
                aiTutorManager.ShowAnalysisFeedback(
                    "AI 後端連線失敗。\n\n" +
                    request.error
                );
            }
        }

        request.Dispose();
    }

    // =========================================================
    // 舊功能：傳送切割數據
    // =========================================================
    public void RequestCutFeedback(
        string task,
        float ratioA,
        float ratioB,
        float difference,
        int cutCount)
    {
        StartCoroutine(
            SendCutRequest(
                task,
                ratioA,
                ratioB,
                difference,
                cutCount
            )
        );
    }

    private IEnumerator SendCutRequest(
        string task,
        float ratioA,
        float ratioB,
        float difference,
        int cutCount)
    {
        CutRequest requestData = new CutRequest
        {
            task = task,
            ratio_a = ratioA,
            ratio_b = ratioB,
            difference = difference,
            cut_count = cutCount
        };

        string json =
            JsonUtility.ToJson(requestData);

        byte[] bodyRaw =
            Encoding.UTF8.GetBytes(json);

        string url =
            backendUrl + "/analyze-cut";

        UnityWebRequest request =
            new UnityWebRequest(
                url,
                UnityWebRequest.kHttpVerbPOST
            );

        request.uploadHandler =
            new UploadHandlerRaw(bodyRaw);

        request.downloadHandler =
            new DownloadHandlerBuffer();

        request.SetRequestHeader(
            "Content-Type",
            "application/json"
        );

        // 最多等 30 秒
        request.timeout = 30;

        Debug.Log(
            "送出切割資料給 AI：" + json
        );

        yield return request.SendWebRequest();

        if (request.result !=
            UnityWebRequest.Result.Success)
        {
            Debug.LogError(
                "AI Tutor API 錯誤：" +
                request.error
            );

            if (aiTutorManager != null)
            {
                aiTutorManager.ShowAnalysisFeedback(
                    "AI 老師暫時沒有連線。\n" +
                    request.error
                );
            }

            request.Dispose();
            yield break;
        }

        HandleAIResponse(
            request.downloadHandler.text
        );

        request.Dispose();
    }

    // =========================================================
    // 玩家視角圖片 → AI
    // =========================================================
    public void RequestImageFeedback(
        byte[] imageBytes,
        int cutCount)
    {
        if (imageBytes == null ||
            imageBytes.Length == 0)
        {
            Debug.LogError(
                "AI Tutor：圖片資料是空的"
            );

            return;
        }

        StartCoroutine(
            SendImageRequest(
                imageBytes,
                cutCount
            )
        );
    }

    private IEnumerator SendImageRequest(
        byte[] imageBytes,
        int cutCount)
    {
        string url =
            backendUrl + "/analyze-image";

        WWWForm form = new WWWForm();

        form.AddBinaryData(
            "image",
            imageBytes,
            "AI_Capture.png",
            "image/png"
        );

        form.AddField(
            "cut_count",
            cutCount
        );

        Debug.Log(
            "正在把玩家視角傳給 AI：" +
            url
        );

        UnityWebRequest request =
            UnityWebRequest.Post(
                url,
                form
            );

        // 避免再出現等五分鐘的狀況
        request.timeout = 60;

        yield return request.SendWebRequest();

        if (request.result !=
            UnityWebRequest.Result.Success)
        {
            Debug.LogError(
                "AI 圖片分析錯誤：" +
                request.error
            );

            if (aiTutorManager != null)
            {
                aiTutorManager.ShowAnalysisFeedback(
                    "AI 老師暫時看不到畫面。\n" +
                    request.error
                );
            }

            request.Dispose();
            yield break;
        }

        Debug.Log(
            "AI 圖片分析原始回覆：" +
            request.downloadHandler.text
        );

        HandleAIResponse(
            request.downloadHandler.text
        );

        request.Dispose();
    }

    // =========================================================
    // 統一處理 AI 回覆
    // =========================================================
    private void HandleAIResponse(
        string responseText)
    {
        AIResponse response =
            JsonUtility.FromJson<AIResponse>(
                responseText
            );

        if (response == null ||
            string.IsNullOrEmpty(response.reply))
        {
            Debug.LogWarning(
                "AI 沒有回傳有效的 reply"
            );

            return;
        }

        Debug.Log(
            "AI Tutor 回覆：" +
            response.reply
        );

        if (aiTutorManager != null)
        {
            aiTutorManager.ShowAnalysisFeedback(
                response.reply
            );
        }
    }
}