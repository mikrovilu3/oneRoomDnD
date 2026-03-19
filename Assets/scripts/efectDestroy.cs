using UnityEngine;

public class EfectDestroy : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Invoke(nameof(Destroyyy), 1f);
    }

    void Destroyyy()
    {
        Destroy(gameObject);
    }
    
}
