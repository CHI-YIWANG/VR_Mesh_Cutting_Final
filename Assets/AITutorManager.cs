using UnityEngine;
using TMPro;
using UnityEngine.InputSystem;

public class AITutorManager : MonoBehaviour
{
    [SerializeField] private TMP_Text tutorText;

    private enum TutorMode
    {
        None,
        Half,
        Triangle
    }

    private TutorMode currentMode = TutorMode.None;
    private int cutCount = 0;

    private void Start()
    {
        TeachHalf();
    }

    private void Update()
    {
        if (Keyboard.current == null)
            return;

        if (Keyboard.current.digit1Key.wasPressedThisFrame)
        {
            TeachHalf();
        }

        if (Keyboard.current.digit2Key.wasPressedThisFrame)
        {
            TeachTriangle();
        }

        if (Keyboard.current.digit0Key.wasPressedThisFrame)
        {
            ShowWelcome();
        }
    }

    public void ShowWelcome()
    {
        currentMode = TutorMode.None;
        cutCount = 0;

        tutorText.text =
            "今天想學怎麼切呢？";
    }

    public void TeachHalf()
    {
        currentMode = TutorMode.Half;
        cutCount = 0;

        tutorText.text =
            "【切成兩半】\n\n" +
            "先找到方塊的中間，\n" +
            "把切割板移到中央，\n" +
            "準備好就切下去！";
    }

    public void TeachTriangle()
    {
        currentMode = TutorMode.Triangle;
        cutCount = 0;

        tutorText.text =
            "【切出三角形】\n\n" +
            "先找到方塊的一個角，\n" +
            "把切割板斜斜地放，\n" +
            "先完成第一刀！";
    }

    public void OnCutCompleted()
    {
        cutCount++;

        if (currentMode == TutorMode.Half)
        {
            if (cutCount == 1)
            {
                tutorText.text =
                    "很好！第一刀完成了！\n\n" +
                    "看看兩塊是不是差不多大。\n" +
                    "如果覺得不平均，\n" +
                    "可以 Undo 再試一次。";
            }
            else if (cutCount == 2)
            {
                tutorText.text =
                    "你又切了一刀！\n\n" +
                    "如果目標只是切成兩半，\n" +
                    "其實第一刀就完成任務囉！";
            }
            else
            {
                tutorText.text =
                    "現在已經切成很多塊了！\n\n" +
                    "想重新練習切成兩半的話，\n" +
                    "可以按 Reset 重新開始。";
            }
        }

        else if (currentMode == TutorMode.Triangle)
        {
            if (cutCount == 1)
            {
                tutorText.text =
                    "很好！第一刀完成了。\n\n" +
                    "現在拿起其中一塊，\n" +
                    "再從另一個角斜切一次。";
            }
            else if (cutCount == 2)
            {
                tutorText.text =
                    "第二刀完成了！\n\n" +
                    "看看留下來的形狀，\n" +
                    "是不是開始像三角形了？";
            }
            else
            {
                tutorText.text =
                    "你又完成一刀！\n\n" +
                    "可以轉一轉手上的形狀，\n" +
                    "看看還需要修哪個角。";
            }
        }
    }
    public void ShowAnalysisFeedback(string message)
    {
        tutorText.text = message;
    }
    public void OnUndo()
    {
        tutorText.text =
            "剛剛那一刀已經復原了！\n\n" +
            "沒關係，我們再試一次。";
    }
    public void OnReset()
    {
        cutCount = 0;

        if (currentMode == TutorMode.Half)
        {
            tutorText.text =
                "【切成兩半】\n\n" +
                "重新開始！\n" +
                "先找到方塊的中間，\n" +
                "再把切割板移到中央。";
        }
        else if (currentMode == TutorMode.Triangle)
        {
            tutorText.text =
                "【切出三角形】\n\n" +
                "重新開始！\n" +
                "先找到方塊的一個角，\n" +
                "把切割板斜斜地放。";
        }
        else
        {
            ShowWelcome();
        }
    }
    public void ShowHint()
    {
        if (currentMode == TutorMode.Half)
        {
            if (cutCount == 0)
            {
                tutorText.text =
                    "提示：\n\n" +
                    "先看看方塊的中間在哪裡，\n" +
                    "把切割板慢慢移到中央。";
            }
            else
            {
                tutorText.text =
                    "提示：\n\n" +
                    "已經切過一刀了！\n" +
                    "如果兩邊看起來不一樣大，\n" +
                    "可以先 Undo 再試一次。";
            }
        }
        else if (currentMode == TutorMode.Triangle)
        {
            if (cutCount == 0)
            {
                tutorText.text =
                    "提示：\n\n" +
                    "先選一個角，\n" +
                    "把切割板斜斜地放。";
            }
            else if (cutCount == 1)
            {
                tutorText.text =
                    "提示：\n\n" +
                    "第一刀完成了！\n" +
                    "拿起其中一塊，\n" +
                    "再從另一個方向切一刀。";
            }
            else
            {
                tutorText.text =
                    "提示：\n\n" +
                    "看看現在的形狀，\n" +
                    "是不是已經有三角形的感覺了？";
            }
        }
        else
        {
            tutorText.text =
                "先選一個想學的任務吧！";
        }
    }
}