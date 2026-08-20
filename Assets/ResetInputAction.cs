using UnityEngine;
using UnityEngine.InputSystem;

public class ResetInputAction : MonoBehaviour
{
    [SerializeField] private InputActionReference resetAction;
    [SerializeField] private ResetManager resetManager;

    private void OnEnable()
    {
        resetAction.action.performed += OnResetPressed;
        resetAction.action.Enable();
    }

    private void OnDisable()
    {
        resetAction.action.performed -= OnResetPressed;
        resetAction.action.Disable();
    }

    private void OnResetPressed(InputAction.CallbackContext context)
    {
        Debug.Log("B 按下：立即 Reset");

        if (resetManager != null)
        {
            resetManager.ResetCube();
        }
    }
}