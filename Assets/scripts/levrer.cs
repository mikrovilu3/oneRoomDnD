using UnityEngine;

public class levrer : MonoBehaviour
{
    public Transform grabable;
    public Transform pivot;
    Quaternion direction;
    public Quaternion ofset;
    public void letGo()
    {
        grabable.position=transform.position;
    }
    // Update is called once per frame
    void FixedUpdate()
    {
        direction.eulerAngles = Vector3.Normalize(grabable.position-pivot.position)+pivot.position;
        pivot.rotation = direction * ofset;
    }
}
