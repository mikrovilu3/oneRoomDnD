using UnityEngine;

public class LeverDie : MonoBehaviour
{
    HingeJoint joint;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        joint = gameObject.GetComponent<HingeJoint>();
    }

    // Update is called once per frame
    void Update()
    {
        if (joint==null || joint.angle >= 100)
        {

        }
    }
}
