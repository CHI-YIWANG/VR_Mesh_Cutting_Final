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

    // 玩家視角 AI 截圖
    [SerializeField] private AIVisionCapture aiVisionCapture;

    private GameObject currentTarget;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Cuttable") ||
            other.CompareTag("CutPiece"))
        {
            currentTarget = other.gameObject;

            Debug.Log(
                "進入切割區：" +
                currentTarget.name
            );
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

    // =========================================================
    // 確認切割
    // =========================================================
    public void ConfirmCut()
    {
        if (currentTarget == null)
        {
            Debug.Log(
                "目前沒有可以切割的物件"
            );

            return;
        }

        SliceTarget();
    }

    // =========================================================
    // 執行切割
    // =========================================================
    private void SliceTarget()
    {
        if (cuttingPlane == null)
        {
            Debug.LogError(
                "MeshCutter 尚未指定 Cutting Plane"
            );

            return;
        }

        GameObject target =
            currentTarget;

        // 使用校正後的切割角度
        Vector3 sliceNormal =
            GetSnappedPlaneNormal();

        SlicedHull hull =
            target.Slice(
                cuttingPlane.position,
                sliceNormal
            );

        if (hull == null)
        {
            Debug.LogWarning(
                "切割失敗：切割平面可能沒有真正穿過物件"
            );

            return;
        }

        // 建立上半部
        GameObject upperHull =
            hull.CreateUpperHull(
                target,
                crossSectionMaterial
            );

        // 建立下半部
        GameObject lowerHull =
            hull.CreateLowerHull(
                target,
                crossSectionMaterial
            );

        if (upperHull == null ||
            lowerHull == null)
        {
            Debug.LogWarning(
                "切割失敗：無法建立切割後物件"
            );

            return;
        }

        upperHull.name =
            target.name + "_Upper";

        lowerHull.name =
            target.name + "_Lower";

        // 設定切割後物件
        SetupCutPiece(
            upperHull,
            cuttingPlane.up
        );

        SetupCutPiece(
            lowerHull,
            -cuttingPlane.up
        );

        // =====================================================
        // Undo
        // =====================================================

        if (undoManager != null)
        {
            undoManager.SaveCut(
                target,
                upperHull,
                lowerHull
            );
        }

        currentTarget = null;

        // Undo 還需要原始物件
        // 所以這裡不 Destroy
        target.SetActive(false);

        // =====================================================
        // 告訴 AI Tutor：完成一刀
        // =====================================================

        if (aiTutorManager != null)
        {
            aiTutorManager.OnCutCompleted();
        }
        // =====================================================
        // 玩家視角 AI 截圖
        // =====================================================

        if (aiVisionCapture != null)
        {
            StartCoroutine(
                CaptureAfterCut()
            );
        }
        else
        {
            Debug.LogWarning(
                "MeshCutter 尚未指定 AIVisionCapture"
            );
        }

        Debug.Log("切割成功！");
    }

    // =========================================================
    // 切割後等待一幀，再拍玩家視角
    // =========================================================

    private System.Collections.IEnumerator CaptureAfterCut()
    {
        // 等待一幀
        // 讓新的 CutPiece 先出現在畫面上
        yield return null;

        if (aiVisionCapture != null)
        {
            Debug.Log(
                "準備擷取切割後的玩家視角..."
            );

            aiVisionCapture
                .CaptureAndSendToAI();
        }
    }

    // =========================================================
    // 校正切割角度
    // =========================================================

    private Vector3 GetSnappedPlaneNormal()
    {
        Vector3 angles =
            cuttingPlane.eulerAngles;

        float snappedX =
            SnapAngle(
                angles.x
            );

        float snappedZ =
            SnapAngle(
                angles.z
            );

        Quaternion snappedRotation =
            Quaternion.Euler(
                snappedX,
                angles.y,
                snappedZ
            );

        return snappedRotation *
               Vector3.up;
    }

    private float SnapAngle(
        float angle
    )
    {
        if (angle > 180f)
        {
            angle -= 360f;
        }

        return Mathf.Round(
            angle / 45f
        ) * 45f;
    }

    // =========================================================
    // 設定切割後的物件
    // =========================================================

    private void SetupCutPiece(
        GameObject piece,
        Vector3 pushDirection
    )
    {
        if (piece == null)
        {
            return;
        }

        // 切割後仍可以再次切割
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

        // =====================================================
        // Collider
        // =====================================================

        MeshCollider meshCollider =
            piece.AddComponent<MeshCollider>();

        meshCollider.sharedMesh =
            meshFilter.sharedMesh;

        meshCollider.convex = true;

        // =====================================================
        // Rigidbody
        // =====================================================

        Rigidbody rb =
            piece.AddComponent<Rigidbody>();

        rb.mass = 0.5f;
        rb.useGravity = true;
        rb.isKinematic = false;

        rb.collisionDetectionMode =
            CollisionDetectionMode.Continuous;

        // 稍微把兩塊推開
        rb.AddForce(
            pushDirection.normalized *
            0.08f,
            ForceMode.Impulse
        );

        // =====================================================
        // VR Grab
        // =====================================================

        XRGrabInteractable grab =
            piece.AddComponent<
                XRGrabInteractable
            >();

        grab.colliders.Add(
            meshCollider
        );

        // 第一次抓起再放開後固定
        piece.AddComponent<
            CutPieceFreezeAfterGrab
        >();
    }
}