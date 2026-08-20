using System.Collections;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

public class AITutorAPI : MonoBehaviour
{
    [SerializeField] private AITutorManager aiTutorManager;

    [SerializeField]
    private string backendUrl =
        "http://127.0.0.1:8000/analyze-cut";

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
        CutRequest requestData = new CutRequest();

        requestData.task = task;
        requestData.ratio_a = ratioA;
        requestData.ratio_b = ratioB;
        requestData.difference = difference;
        requestData.cut_count = cutCount;

        string json = JsonUtility.ToJson(requestData);

        byte[] bodyRaw =
            Encoding.UTF8.GetBytes(json);

        UnityWebRequest request =
            new UnityWebRequest(
                backendUrl,
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

        Debug.Log(
            "送出 AI Tutor Request：" + json
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
                    "AI 老師暫時沒有連線，請再試一次。"
                );
            }

            request.Dispose();
            yield break;
        }

        string responseText =
            request.downloadHandler.text;

        Debug.Log(
            "AI Tutor 原始回覆：" +
            responseText
        );

        AIResponse response =
            JsonUtility.FromJson<AIResponse>(
                responseText
            );

        if (response != null &&
            !string.IsNullOrEmpty(response.reply))
        {
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

        request.Dispose();
    }
}