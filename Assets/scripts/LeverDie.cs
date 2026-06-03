using UnityEngine;

public class LeverDie : MonoBehaviour
{
    public HingeJoint joint;
    public GameObject arrow;
    public Vector3 arrowDirection;
    public float arrowSpread;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        joint = gameObject.GetComponent<HingeJoint>();
    }

    // Update is called once per frame
    void Update()
    {
        if (joint==null || joint.angle <= 0)
        {
            Instantiate(arrow,new Vector3(0,0,0),);

        }
    }
}
