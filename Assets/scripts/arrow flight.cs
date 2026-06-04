using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class Arrow : MonoBehaviour
{
    [Tooltip("Damage dealt on impact.")]
    public float damage = 25f;

    [Tooltip("Layers the arrow can stick into.")]
    public LayerMask stickLayers = ~0;

    [Tooltip("Which local axis points along the arrow shaft toward the tip. " +
             "Z = default Unity forward. Change to Y if your model points up.")]
    public ArrowAxis shaftAxis = ArrowAxis.Z;

    public enum ArrowAxis { X, Y, Z }

    private Rigidbody _rb;
    private bool _stuck;

    void Awake() => _rb = GetComponent<Rigidbody>();

    void FixedUpdate()
    {
        if (_stuck || _rb.linearVelocity.sqrMagnitude < 0.01f) return;

        // Rotate so the chosen shaft axis aligns with the velocity vector.
        Quaternion targetRot = VelocityToRotation(_rb.linearVelocity.normalized);
        _rb.MoveRotation(targetRot);           // instant, no slerp fighting physics
    }

    void OnCollisionEnter(Collision col)
    {
        if (_stuck) return;
        if ((stickLayers.value & (1 << col.gameObject.layer)) == 0) return;

        col.gameObject.GetComponent<IDamageable>()?.TakeDamage(damage);
        Stick(col);
    }

    void Stick(Collision col)
    {
        _stuck = true;
        _rb.linearVelocity = Vector3.zero;
        _rb.angularVelocity = Vector3.zero;
        _rb.isKinematic = true;
        transform.SetParent(col.transform, worldPositionStays: true);
    }

    Quaternion VelocityToRotation(Vector3 dir)
    {
        return shaftAxis switch
        {
            // LookRotation points Z toward dir.
            // For X or Y shafts we apply an additional fixed offset.
            ArrowAxis.Z => Quaternion.LookRotation(dir),
            ArrowAxis.Y => Quaternion.LookRotation(dir) * Quaternion.Euler(-90f, 0f, 0f),
            ArrowAxis.X => Quaternion.LookRotation(dir) * Quaternion.Euler(0f, -90f, 0f),
            _ => Quaternion.LookRotation(dir),
        };
    }
}