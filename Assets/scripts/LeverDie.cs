using System.Collections;
using UnityEngine;
//using UnityEngine.WSA;

public class SpawnAndArc : MonoBehaviour
{
    public HingeJoint joint;

    public GameObject prefab;
    public Transform player;

    [Tooltip("World-space offset from the player where the projectile spawns.")]
    public Vector3 spawnOffset = new Vector3(5f, 10f, 5f);

    [Tooltip("Launch angle in degrees (45 = equal range/height trade-off).")]
    [Range(1f, 89f)]
    public float launchAngle = 45f;

    [Header("Wave Settings")]
    public int countPerWave = 5;
    public float spawnInterval = 0.3f;
    public float waveInterval = 3f;

    [Header("Near-Miss Settings")]
    [Tooltip("Max radius (world units) near-miss shots deviate from the player.")]
    public float nearMissRadius = 2f;
    float launched;

    void Start()
    {
        joint = gameObject.GetComponent<HingeJoint>();
    }

    void Update()
    {
        if ((joint == null || joint.angle <= 0) && Time.time - launched > 10)
        {
            TriggerWave();

        }
    }


    public void TriggerWave() => StartCoroutine(SpawnWave());

    IEnumerator SpawnWave()
    {
        launched = Time.time;
        // Pick a random slot to be the guaranteed hit.
        int hitSlot = Random.Range(0, countPerWave);

        for (int i = 0; i < countPerWave; i++)
        {
            Vector3 spawnPos = player.position + spawnOffset;
            Vector3 aimTarget;

            if (i == hitSlot)
            {
                aimTarget = player.position;
            }
            else
            {
                // Random point on the plane perpendicular to the incoming direction.
                Vector3 toSpawn = (spawnPos - player.position).normalized;
                Vector3 right = Vector3.Cross(toSpawn, Vector3.up).normalized;
                Vector3 up = Vector3.Cross(right, toSpawn).normalized;

                float minMiss = nearMissRadius * 0.3f;
                float r = Random.Range(minMiss, nearMissRadius);
                float angle = Random.Range(0f, Mathf.PI * 2f);

                aimTarget = player.position+ right * (r * Mathf.Cos(angle))+ up * ((r * Mathf.Sin(angle))+2);
            }

            if (TryCalculateVelocity(spawnPos, aimTarget, launchAngle, out Vector3 velocity))
            {
                GameObject go = Instantiate(prefab, spawnPos, Quaternion.LookRotation(velocity.normalized));
                Rigidbody rb = go.GetComponent<Rigidbody>();
                rb.useGravity = true;
                rb.linearVelocity = velocity;
            }
            else
            {
                Debug.LogWarning($"[SpawnAndArc] No valid arc for projectile {i} — skipped.");
            }

            yield return new WaitForSeconds(spawnInterval);
        }
    }

    static bool TryCalculateVelocity(Vector3 origin, Vector3 target,float angleDeg, out Vector3 velocity)
    {
        float g = Mathf.Abs(Physics.gravity.y);
        float rad = angleDeg * Mathf.Deg2Rad;

        Vector3 horizontal = new Vector3(target.x - origin.x, 0f, target.z - origin.z);
        float flatDist = horizontal.magnitude;
        float deltaY = target.y - origin.y;

        float denom = flatDist * Mathf.Tan(rad) - deltaY;

        if (denom <= 0f)
        {
            velocity = Vector3.zero;
            return false;
        }

        float vHorizontal = Mathf.Sqrt((0.5f * g * flatDist * flatDist) / denom);
        float vVertical = vHorizontal * Mathf.Tan(rad);

        velocity = horizontal.normalized * vHorizontal + Vector3.up * vVertical;
        return true;
    }
}