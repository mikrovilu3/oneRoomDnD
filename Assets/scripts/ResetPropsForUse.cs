using UnityEditor.ShaderGraph.Internal;
using UnityEngine;

public class ResetPropsForUse : MonoBehaviour
{
    public Vector3 NeededScale = Vector3.one;
    
    public void Rescale()
    {
        Debug.Log("before scaleing "+transform.localScale);
        transform.localScale = NeededScale;
        Debug.Log("after scaleing " + transform.localScale);
    }
}
