using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using System;
using System.IO;

public class UserActionLogger : MonoBehaviour
{
    public static UserActionLogger Instance { get; private set; }

    [Header("要記錄抓取的物件")]
    public XRGrabInteractable[] trackedObjects;

    private string filePath;
    private string sessionId;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        // 本次使用的 Session ID
        sessionId = DateTime.Now.ToString("yyyyMMdd_HHmmss");

        // 固定使用同一個檔名
        filePath = Path.Combine(
            Application.persistentDataPath,
            "UserActionLog.txt"
        );

        // 每次重新啟動 App 時：
        // 重新建立檔案，上一輪紀錄會在這一刻被取代
        File.WriteAllText(
            filePath,
            "========== 使用者操作紀錄 ==========\n" +
            $"Session ID：{sessionId}\n" +
            $"開始時間：{DateTime.Now:yyyy-MM-dd HH:mm:ss}\n" +
            "====================================\n\n"
        );

        Debug.Log("UserActionLogger 啟動");
        Debug.Log("本次為全新的操作紀錄");
        Debug.Log("紀錄檔位置：" + filePath);

        LogAction(
            "SessionStart",
            "System",
            "使用者開始本次操作"
        );
    }

    private void Start()
    {
        if (trackedObjects == null || trackedObjects.Length == 0)
        {
            Debug.LogWarning("UserActionLogger：沒有設定 Tracked Objects");
            return;
        }

        foreach (XRGrabInteractable grab in trackedObjects)
        {
            if (grab == null)
            {
                Debug.LogWarning("UserActionLogger：發現空的 Tracked Object");
                continue;
            }

            XRGrabInteractable currentGrab = grab;

            currentGrab.selectEntered.AddListener(
                args => OnGrab(currentGrab)
            );

            currentGrab.selectExited.AddListener(
                args => OnRelease(currentGrab)
            );

            Debug.Log(
                "開始監聽物件：" + currentGrab.gameObject.name
            );
        }
    }

    private void OnGrab(XRGrabInteractable grab)
    {
        if (grab == null)
            return;

        LogAction(
            "Grab",
            grab.gameObject.name,
            "使用者抓起物件"
        );
    }

    private void OnRelease(XRGrabInteractable grab)
    {
        if (grab == null)
            return;

        LogAction(
            "Release",
            grab.gameObject.name,
            "使用者放開物件"
        );
    }

    public void LogAction(
        string action,
        string objectName,
        string details = "")
    {
        if (string.IsNullOrEmpty(filePath))
        {
            Debug.LogWarning("UserActionLogger：紀錄檔尚未建立");
            return;
        }

        string time =
            DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

        string line =
            $"[{time}] [{action}] {objectName} - {details}\n";

        File.AppendAllText(filePath, line);

        Debug.Log(
            $"[User Log] {action} | {objectName} | {details}"
        );
    }
}