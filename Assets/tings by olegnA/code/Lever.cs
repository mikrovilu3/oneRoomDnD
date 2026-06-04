using UnityEngine;

public class VRLever : MonoBehaviour
{
    [Header("Door")]
    [SerializeField] private DoorController targetDoor;

    [Header("Lever Settings")]
    [SerializeField] private float openThreshold = -30f;

    private bool leverOn;

    private void Update()
    {
        float currentAngle = NormalizeAngle(transform.localEulerAngles.x);

        if (!leverOn && currentAngle <= openThreshold)
        {
            leverOn = true;

            if (targetDoor != null)
                targetDoor.OpenDoor();
        }
        else if (leverOn && currentAngle > openThreshold)
        {
            leverOn = false;

            if (targetDoor != null)
                targetDoor.CloseDoor();
        }
    }

    private float NormalizeAngle(float angle)
    {
        if (angle > 180f)
            angle -= 360f;

        return angle;
    }
}