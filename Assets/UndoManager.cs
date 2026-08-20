using UnityEngine;

public class UndoManager : MonoBehaviour
{
    [SerializeField] private AITutorManager aiTutorManager;

    private GameObject objectBeforeCut;
    private GameObject cutPieceA;
    private GameObject cutPieceB;

    public void SaveCut(
        GameObject beforeCut,
        GameObject pieceA,
        GameObject pieceB)
    {
        objectBeforeCut = beforeCut;
        cutPieceA = pieceA;
        cutPieceB = pieceB;

        Debug.Log("已記錄上一刀");
    }

    public void UndoLastCut()
    {
        Debug.Log("Undo 被執行");

        if (objectBeforeCut == null)
        {
            Debug.Log("目前沒有可以 Undo 的切割");
            return;
        }

        objectBeforeCut.SetActive(true);

        if (cutPieceA != null)
            Destroy(cutPieceA);

        if (cutPieceB != null)
            Destroy(cutPieceB);

        objectBeforeCut = null;
        cutPieceA = null;
        cutPieceB = null;

        if (aiTutorManager != null)
        {
            aiTutorManager.OnUndo();
        }

        Debug.Log("上一刀已復原");
    }
}