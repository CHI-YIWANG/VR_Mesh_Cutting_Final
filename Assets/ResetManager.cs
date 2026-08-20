using UnityEngine;

public class ResetManager : MonoBehaviour
{
    [Header("原始 Cube Prefab")]
    [SerializeField] private GameObject cubePrefab;

    [Header("重生位置")]
    [SerializeField] private Transform spawnPoint;

    [Header("AI 教學")]
    [SerializeField] private AITutorManager aiTutorManager;

    public void ResetCube()
    {
        Debug.Log("開始Reset");

        GameObject original =
            GameObject.FindGameObjectWithTag("Cuttable");

        if (original != null)
        {
            Destroy(original);
        }

        GameObject[] pieces =
            GameObject.FindGameObjectsWithTag("CutPiece");

        foreach (GameObject piece in pieces)
        {
            Destroy(piece);
        }

        GameObject newCube = Instantiate(
            cubePrefab,
            spawnPoint.position,
            spawnPoint.rotation
        );

        newCube.name = "CutTarget_Cube";

        if (aiTutorManager != null)
        {
            aiTutorManager.OnReset();
        }

        Debug.Log("Cube 已重置");
    }
}