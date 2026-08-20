using UnityEngine;
using UnityEngine.InputSystem;

public class UndoInputAction : MonoBehaviour
{
    [SerializeField] private InputActionReference undoAction;
    [SerializeField] private UndoManager undoManager;

    private void OnEnable()
    {
        if (undoAction == null)
        {
            Debug.LogError("Undo Action 沒有指定！");
            return;
        }

        undoAction.action.performed += OnUndoPressed;
        undoAction.action.Enable();

        Debug.Log("Undo Input Action 已啟用：" + undoAction.action.name);
    }

    private void OnDisable()
    {
        if (undoAction == null)
            return;

        undoAction.action.performed -= OnUndoPressed;
        undoAction.action.Disable();
    }

    private void OnUndoPressed(InputAction.CallbackContext context)
    {
        Debug.Log("★★★ A 鍵有被偵測到 ★★★");

        if (undoManager == null)
        {
            Debug.LogError("Undo Manager 沒有指定！");
            return;
        }

        undoManager.UndoLastCut();
    }
}