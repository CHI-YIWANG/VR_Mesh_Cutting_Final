using UnityEngine;
using UnityEngine.InputSystem;

public class HintInputAction : MonoBehaviour
{
    [SerializeField] private InputActionReference hintAction;
    [SerializeField] private AITutorManager aiTutorManager;

    private void OnEnable()
    {
        if (hintAction == null)
            return;

        hintAction.action.performed += OnHintPressed;
        hintAction.action.Enable();
    }

    private void OnDisable()
    {
        if (hintAction == null)
            return;

        hintAction.action.performed -= OnHintPressed;
        hintAction.action.Disable();
    }

    private void OnHintPressed(InputAction.CallbackContext context)
    {
        if (aiTutorManager != null)
        {
            aiTutorManager.ShowHint();
        }
    }
}