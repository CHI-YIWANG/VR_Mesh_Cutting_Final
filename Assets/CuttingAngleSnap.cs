using UnityEngine;

public class CuttingAngleSnap : MonoBehaviour
{
    [SerializeField] private Transform cuttingPlane;

    private bool insideCuttable;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Cuttable") ||
            other.CompareTag("CutPiece"))
        {
            insideCuttable = true;
            Debug.Log("進入切割準備區");
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Cuttable") ||
            other.CompareTag("CutPiece"))
        {
            insideCuttable = false;
            Debug.Log("離開切割準備區");
        }
    }

    private void LateUpdate()
    {
        if (!insideCuttable || cuttingPlane == null)
            return;

        Vector3 angles = cuttingPlane.eulerAngles;

        float snappedX = SnapAngle(angles.x);
        float snappedZ = SnapAngle(angles.z);

        cuttingPlane.rotation = Quaternion.Euler(
            snappedX,
            angles.y,
            snappedZ
        );
    }

    private float SnapAngle(float angle)
    {
        if (angle > 180f)
            angle -= 360f;

        return Mathf.Round(angle / 45f) * 45f;
    }
}