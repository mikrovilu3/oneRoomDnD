using System.Collections;
using UnityEngine;

public class DoorController : MonoBehaviour
{
    [Header("Door")]
    [SerializeField] private Transform doorPivot;
    [SerializeField] private float openAngle = 90f;
    [SerializeField] private float moveSpeed = 180f;

    private Quaternion closedRotation;
    private Quaternion openRotation;

    private bool isOpen;
    private Coroutine moveRoutine;

    private void Awake()
    {
        if (doorPivot == null)
            doorPivot = transform;

        closedRotation = doorPivot.localRotation;
        openRotation = closedRotation * Quaternion.Euler(0f, openAngle, 0f);
    }

    public void OpenDoor()
    {
        if (isOpen)
            return;

        isOpen = true;
        MoveTo(openRotation);
    }

    public void CloseDoor()
    {
        if (!isOpen)
            return;

        isOpen = false;
        MoveTo(closedRotation);
    }

    public void ToggleDoor()
    {
        if (isOpen)
            CloseDoor();
        else
            OpenDoor();
    }

    private void MoveTo(Quaternion target)
    {
        if (moveRoutine != null)
            StopCoroutine(moveRoutine);

        moveRoutine = StartCoroutine(RotateDoor(target));
    }

    private IEnumerator RotateDoor(Quaternion target)
    {
        while (Quaternion.Angle(doorPivot.localRotation, target) > 0.1f)
        {
            doorPivot.localRotation = Quaternion.RotateTowards(
                doorPivot.localRotation,
                target,
                moveSpeed * Time.deltaTime
            );

            yield return null;
        }

        doorPivot.localRotation = target;
    }
}