using UnityEngine;
using EzySlice;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

public class MeshCutter : MonoBehaviour
{
    [SerializeField] private Transform cuttingPlane;
    [SerializeField] private Material crossSectionMaterial;
    [SerializeField] private UndoManager undoManager;
    [SerializeField] private AITutorManager aiTutorManager;
    [SerializeField] private CutAnalyzer cutAnalyzer;

    private GameObject currentTarget;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Cuttable") ||
            other.CompareTag("CutPiece"))
        {
            currentTarget = other.gameObject;
            Debug.Log("進入切割區：" + currentTarget.name);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (currentTarget != null &&
            currentTarget == other.gameObject)
        {
            Debug.Log("離開切割區");
            currentTarget = null;
        }
    }

    public void ConfirmCut()
    {
        if (currentTarget == null)
        {
            Debug.Log("目前沒有可以切割的物件");
            return;
        }

        SliceTarget();
    }

    private void SliceTarget()
    {
        if (cuttingPlane == null)
        {
            Debug.LogError("MeshCutter 尚未指定 Cutting Plane");
            return;
        }

        GameObject target = currentTarget;

        SlicedHull hull = target.Slice(
            cuttingPlane.position,
            cuttingPlane.up
        );

        if (hull == null)
        {
            Debug.LogWarning(
                "切割失敗：切割平面可能沒有真正穿過物件"
            );
            return;
        }

        GameObject upperHull =
            hull.CreateUpperHull(
                target,
                crossSectionMaterial
            );

        GameObject lowerHull =
            hull.CreateLowerHull(
                target,
                crossSectionMaterial
            );

        if (upperHull == null || lowerHull == null)
        {
            Debug.LogWarning("切割失敗：無法建立切割後物件");
            return;
        }

        upperHull.name =
            target.name + "_Upper";

        lowerHull.name =
            target.name + "_Lower";

        SetupCutPiece(
            upperHull,
            cuttingPlane.up
        );

        SetupCutPiece(
            lowerHull,
            -cuttingPlane.up
        );

        // 保存這一刀，讓 Undo 可以復原
        if (undoManager != null)
        {
            undoManager.SaveCut(
                target,
                upperHull,
                lowerHull
            );
        }

        currentTarget = null;

        // Undo 還需要原本物件，所以不能 Destroy
        target.SetActive(false);

        // 先告訴 AI Tutor：完成了一刀
        if (aiTutorManager != null)
        {
            aiTutorManager.OnCutCompleted();
        }

        // 最後才分析切割結果
        // 之後這裡會把真正的切割資料送到 AI
        if (cutAnalyzer != null)
        {
            cutAnalyzer.AnalyzeHalfCut(
                upperHull,
                lowerHull
            );
        }

        Debug.Log("切割成功！");
    }

    private void SetupCutPiece(
        GameObject piece,
        Vector3 pushDirection)
    {
        if (piece == null)
            return;

        // 切割後仍然可以再次切割
        piece.tag = "CutPiece";

        MeshFilter meshFilter =
            piece.GetComponent<MeshFilter>();

        if (meshFilter == null ||
            meshFilter.sharedMesh == null)
        {
            Debug.LogWarning(
                "切割物件缺少 MeshFilter 或 Mesh"
            );
            return;
        }

        // 建立新的 Collider
        MeshCollider meshCollider =
            piece.AddComponent<MeshCollider>();

        meshCollider.sharedMesh =
            meshFilter.sharedMesh;

        meshCollider.convex = true;

        // 切完後先自由掉落
        Rigidbody rb =
            piece.AddComponent<Rigidbody>();

        rb.mass = 0.5f;
        rb.useGravity = true;
        rb.isKinematic = false;

        rb.collisionDetectionMode =
            CollisionDetectionMode.Continuous;

        // 稍微推開兩塊，讓切割效果更明顯
        rb.AddForce(
            pushDirection.normalized * 0.08f,
            ForceMode.Impulse
        );

        // 讓切割後的物件可以被 VR 抓取
        XRGrabInteractable grab =
            piece.AddComponent<XRGrabInteractable>();

        grab.colliders.Add(meshCollider);

        // 第一次抓起再放開後固定
        piece.AddComponent<CutPieceFreezeAfterGrab>();
    }
}