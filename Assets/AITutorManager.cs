using UnityEngine;
using TMPro;
using UnityEngine.InputSystem;

public class AITutorManager : MonoBehaviour
{
    [SerializeField] private TMP_Text tutorText;

    private int cutCount = 0;

    private void Start()
    {
        ShowMission();
    }

    private void Update()
    {
        if (Keyboard.current == null)
            return;

        // Editor 測試：按 0 重新顯示任務
        if (Keyboard.current.digit0Key.wasPressedThisFrame)
        {
            ShowMission();
        }
    }

    // =========================================================
    // 一開始黑板顯示的任務
    // =========================================================
    public void ShowMission()
    {
        cutCount = 0;

        tutorText.text =
            "【今日任務】\n\n" +
            "請將正方形切成梯形。\n\n" +
            "讓上面比較短，下面比較長，\n" +
            "左右兩邊形成斜邊。\n\n" +
            "想想第一刀要從哪邊開始吧！";
    }

    // =========================================================
    // 每完成一次切割
    // =========================================================
    public void OnCutCompleted()
    {
        cutCount++;

        tutorText.text =
            "第 " + cutCount + " 次切割完成！\n\n" +
            "AI 老師正在觀察你的作品...";
    }

    // =========================================================
    // AI 回覆後顯示在黑板
    // =========================================================
    public void ShowAnalysisFeedback(string message)
    {
        tutorText.text = message;
    }

    // =========================================================
    // Reset
    // =========================================================
    public void OnReset()
    {
        ShowMission();
    }

    // =========================================================
    // 提示
    // =========================================================
    public void ShowHint()
    {
        if (cutCount == 0)
        {
            tutorText.text =
                "【提示】\n\n" +
                "觀察正方形的左右兩側，\n" +
                "想想先從哪一邊切出斜邊。";
        }
        else if (cutCount == 1)
        {
            tutorText.text =
                "【提示】\n\n" +
                "已經完成一邊了！\n" +
                "接著觀察另一邊，\n" +
                "試著切出相近的斜度。";
        }
        else
        {
            tutorText.text =
                "兩次切割完成！\n\n" +
                "看看現在的外輪廓：\n" +
                "上面是不是比較短、下面比較長，\n" +
                "左右兩邊是不是都有斜邊？";
        }
    }

    // =========================================================
    // 保留 Undo，避免其他腳本呼叫時出錯
    // =========================================================
    public void OnUndo()
    {
        if (cutCount > 0)
        {
            cutCount--;
        }

        tutorText.text =
            "剛剛的切割已經復原。\n\n" +
            "再觀察一下作品，\n" +
            "準備好再試一次！";
    }
}