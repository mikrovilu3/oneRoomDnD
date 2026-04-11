using UnityEngine;

/// <summary>
/// Simple fly-cam controller for exploring the generated terrain.
/// Attach to the player GameObject (which also has a Camera child or Camera component).
///
/// Controls:
///   WASD / Arrow Keys  — Move
///   Q / E              — Move down / up
///   Mouse drag (RMB)   — Look
///   Scroll wheel       — Change move speed
///   Shift              — Sprint
/// </summary>
public class PlayerController : MonoBehaviour
{
    [Header("Look")]
    public float mouseSensitivity = 2.5f;
    public float lookSmoothing    = 5f;

    [Header("Move")]
    public float moveSpeed      = 30f;
    public float sprintMultiplier = 4f;
    public float moveSmoothing  = 8f;

    [Header("Terrain Following")]
    [Tooltip("When enabled, the player hovers above terrain at hoverHeight.")]
    public bool  followTerrain  = false;
    public float hoverHeight    = 5f;

    // ─── Private ───────────────────────────────────────────────────────────────

    float   yaw;
    float   pitch;
    Vector3 velocity;

    void Start()
    {
        var angles = transform.eulerAngles;
        yaw   = angles.y;
        pitch = angles.x;
        Cursor.lockState = CursorLockMode.None;
    }

    void Update()
    {
        HandleLook();
        HandleMovement();

        if (followTerrain) SnapToTerrain();
    }

    // ─── Look ──────────────────────────────────────────────────────────────────

    void HandleLook()
    {
        if (!Input.GetMouseButton(1)) return; // RMB held to look

        float mx = Input.GetAxis("Mouse X") * mouseSensitivity;
        float my = Input.GetAxis("Mouse Y") * mouseSensitivity;

        yaw   += mx;
        pitch -= my;
        pitch  = Mathf.Clamp(pitch, -89f, 89f);

        transform.rotation = Quaternion.Euler(pitch, yaw, 0f);
    }

    // ─── Movement ──────────────────────────────────────────────────────────────

    void HandleMovement()
    {
        // Speed adjustment via scroll
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        moveSpeed = Mathf.Clamp(moveSpeed + scroll * moveSpeed * 0.3f, 1f, 2000f);

        float speed = moveSpeed * (Input.GetKey(KeyCode.LeftShift) ? sprintMultiplier : 1f);

        Vector3 input = Vector3.zero;
        input += transform.forward  * Input.GetAxis("Vertical");
        input += transform.right    * Input.GetAxis("Horizontal");
        input += Vector3.up         * (Input.GetKey(KeyCode.E) ? 1f : Input.GetKey(KeyCode.Q) ? -1f : 0f);

        velocity = Vector3.Lerp(velocity, input.normalized * speed, Time.deltaTime * moveSmoothing);
        transform.position += velocity * Time.deltaTime;
    }

    // ─── Terrain following ────────────────────────────────────────────────────

    void SnapToTerrain()
    {
        Ray ray = new Ray(transform.position + Vector3.up * 500f, Vector3.down);
        if (Physics.Raycast(ray, out RaycastHit hit, 1000f))
        {
            float targetY = hit.point.y + hoverHeight;
            transform.position = new Vector3(transform.position.x, targetY, transform.position.z);
        }
    }
}
