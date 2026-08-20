using UnityEngine;
using UnityEngine.XR;

public class RightTriggerCutInput : MonoBehaviour
{
    [SerializeField] private MeshCutter meshCutter;

    private InputDevice rightHand;
    private bool wasPressed;

    private void Start()
    {
        rightHand = InputDevices.GetDeviceAtXRNode(XRNode.RightHand);
    }

    private void Update()
    {
        if (!rightHand.isValid)
        {
            rightHand = InputDevices.GetDeviceAtXRNode(XRNode.RightHand);
            return;
        }

        if (rightHand.TryGetFeatureValue(
            CommonUsages.triggerButton,
            out bool isPressed))
        {
            // 只在「剛按下去」那一瞬間切一次
            if (isPressed && !wasPressed)
            {
                if (meshCutter != null)
                {
                    meshCutter.ConfirmCut();
                }
            }

            wasPressed = isPressed;
        }
    }
}