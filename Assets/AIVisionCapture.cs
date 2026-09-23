using UnityEngine;
using UnityEngine.InputSystem;
using System.IO;

public class AIVisionCapture : MonoBehaviour
{
    [Header("玩家視角 Camera")]
    [SerializeField] private Camera playerCamera;

    [Header("AI Tutor API")]
    [SerializeField] private AITutorAPI aiTutorAPI;

    private int cutCount = 0;

    private void Update()
    {
        // Unity Editor 測試用
        // 按 P → 擷取玩家視角 → 傳給 AI
        if (Keyboard.current != null &&
            Keyboard.current.pKey.wasPressedThisFrame)
        {
            CaptureAndSendToAI();
        }
    }

    // =========================================================
    // 擷取玩家視角 + 傳送給 AI
    // =========================================================
    public void CaptureAndSendToAI()
    {
        if (playerCamera == null)
        {
            Debug.LogError(
                "AIVisionCapture：尚未指定 Player Camera"
            );
            return;
        }

        if (aiTutorAPI == null)
        {
            Debug.LogError(
                "AIVisionCapture：尚未指定 AITutorAPI"
            );
            return;
        }

        cutCount++;

        byte[] imageBytes = CapturePlayerView();

        if (imageBytes == null ||
            imageBytes.Length == 0)
        {
            Debug.LogError(
                "AIVisionCapture：玩家視角截圖失敗"
            );
            return;
        }

        Debug.Log(
            "第 " + cutCount +
            " 次切割：玩家視角截圖完成，準備傳送給 AI..."
        );

        aiTutorAPI.RequestImageFeedback(
            imageBytes,
            cutCount
        );
    }

    // =========================================================
    // 擷取玩家目前看到的畫面
    // =========================================================
    private byte[] CapturePlayerView()
    {
        RenderTexture renderTexture =
            new RenderTexture(
                512,
                512,
                24
            );

        Texture2D image =
            new Texture2D(
                512,
                512,
                TextureFormat.RGB24,
                false
            );

        RenderTexture previousActive =
            RenderTexture.active;

        RenderTexture previousTarget =
            playerCamera.targetTexture;

        try
        {
            playerCamera.targetTexture =
                renderTexture;

            RenderTexture.active =
                renderTexture;

            // 使用玩家目前的 Camera 位置與方向拍攝
            playerCamera.Render();

            image.ReadPixels(
                new Rect(
                    0,
                    0,
                    512,
                    512
                ),
                0,
                0
            );

            image.Apply();

            byte[] bytes =
                image.EncodeToPNG();

            // 每一刀分開保存
            string path =
                Path.Combine(
                Application.persistentDataPath,
                "AI_Capture.png"
                );

            File.WriteAllBytes(
                path,
                bytes
            );

            Debug.Log(
                "玩家視角截圖已保存：" +
                path
            );

            return bytes;
        }
        finally
        {
            playerCamera.targetTexture =
                previousTarget;

            RenderTexture.active =
                previousActive;

            renderTexture.Release();

            Destroy(renderTexture);
            Destroy(image);
        }
    }

    // =========================================================
    // Reset 時讓截圖編號重新開始
    // =========================================================
    public void ResetCaptureCount()
    {
        cutCount = 0;

        Debug.Log(
            "AI 截圖次數已重設"
        );
    }
}