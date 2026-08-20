using UnityEngine;

public class CutAnalyzer : MonoBehaviour
{
    [SerializeField] private AITutorAPI aiTutorAPI;

    private int cutCount = 0;

    public void AnalyzeHalfCut(GameObject pieceA, GameObject pieceB)
    {
        if (pieceA == null || pieceB == null)
        {
            Debug.LogWarning("CutAnalyzer：切割物件不存在");
            return;
        }

        float volumeA = EstimateVolume(pieceA);
        float volumeB = EstimateVolume(pieceB);

        float totalVolume = volumeA + volumeB;

        if (totalVolume <= 0f)
        {
            Debug.LogWarning("CutAnalyzer：無法計算體積");
            return;
        }

        float ratioA = volumeA / totalVolume;
        float ratioB = volumeB / totalVolume;
        float difference = Mathf.Abs(ratioA - ratioB);

        cutCount++;

        Debug.Log(
            "===== CutAnalyzer =====\n" +
            "A 比例：" + ratioA.ToString("F2") + "\n" +
            "B 比例：" + ratioB.ToString("F2") + "\n" +
            "差距：" + difference.ToString("F2") + "\n" +
            "第幾刀：" + cutCount
        );

        if (aiTutorAPI != null)
        {
            aiTutorAPI.RequestCutFeedback(
                "half",
                ratioA,
                ratioB,
                difference,
                cutCount
            );
        }
        else
        {
            Debug.LogWarning(
                "CutAnalyzer 尚未指定 AITutorAPI"
            );
        }
    }

    private float EstimateVolume(GameObject piece)
    {
        MeshFilter meshFilter =
            piece.GetComponent<MeshFilter>();

        if (meshFilter == null ||
            meshFilter.sharedMesh == null)
        {
            return 0f;
        }

        Bounds bounds =
            meshFilter.sharedMesh.bounds;

        Vector3 size =
            Vector3.Scale(
                bounds.size,
                piece.transform.lossyScale
            );

        return Mathf.Abs(
            size.x * size.y * size.z
        );
    }
}