using UnityEngine;

public class levrer : MonoBehaviour
{
    public Transform grabable;
    public Transform pivot;
    public void letGo()
    {
        grabable.position=transform.position;
    }
    // Update is called once per frame
    void FixedUpdate()
    {
        transform.position = Vector3.Normalize(grabable.position-pivot.position)+pivot.position;
    }
}
