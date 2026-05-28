using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

// We place the interface here so Unity is guaranteed to compile it.
public interface IDamageable
{
    void TakeDamage(float damage);
}

/// <summary>
/// VR Sword with collision detection, damage dealing, and slash effects.
/// Requires XR Interaction Toolkit.
/// </summary>
[RequireComponent(typeof(UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable))]
[RequireComponent(typeof(Rigidbody))]
public class VRSword : MonoBehaviour
{
    [Header("Sword Settings")]
    [SerializeField] private float baseDamage = 25f;
    [SerializeField] private float minVelocityForDamage = 2f;
    [SerializeField] private LayerMask damageableLayers;

    [Header("Slash Trail")]
    [SerializeField] private TrailRenderer slashTrail;
    [SerializeField] private float trailActiveVelocity = 3f;

    [Header("Audio")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip swingSound;
    [SerializeField] private AudioClip hitSound;
    [SerializeField] private float swingCooldown = 0.3f;

    [Header("Haptics")]
    [SerializeField] private float hitHapticIntensity = 0.5f;
    [SerializeField] private float hitHapticDuration = 0.1f;

    private UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable grabInteractable;
    private Rigidbody rb;
    private Vector3 previousPosition;
    private float lastSwingTime;
    private bool isGrabbed;

    private float trackedVelocity;

    private void Awake()
    {
        grabInteractable = GetComponent<UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable>();
        rb = GetComponent<Rigidbody>();

        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.spatialBlend = 1f;

        if (slashTrail != null)
            slashTrail.emitting = false;

        grabInteractable.selectEntered.AddListener(OnGrabbed);
        grabInteractable.selectExited.AddListener(OnReleased);
    }

    private void OnDestroy()
    {
        grabInteractable.selectEntered.RemoveListener(OnGrabbed);
        grabInteractable.selectExited.RemoveListener(OnReleased);
    }

    private void OnGrabbed(SelectEnterEventArgs args)
    {
        isGrabbed = true;
        previousPosition = transform.position;
        trackedVelocity = 0f;
    }

    private void OnReleased(SelectExitEventArgs args)
    {
        isGrabbed = false;
        trackedVelocity = 0f;

        if (slashTrail != null)
            slashTrail.emitting = false;
    }

    private void Update()
    {
        if (!isGrabbed) return;

        trackedVelocity = (transform.position - previousPosition).magnitude / Time.deltaTime;
        previousPosition = transform.position;

        if (slashTrail != null)
            slashTrail.emitting = trackedVelocity >= trailActiveVelocity;

        if (trackedVelocity >= trailActiveVelocity && Time.time - lastSwingTime > swingCooldown)
        {
            PlaySwingSound();
            lastSwingTime = Time.time;
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (!isGrabbed) return;
        if (trackedVelocity < minVelocityForDamage) return;
        if (((1 << collision.gameObject.layer) & damageableLayers) == 0) return;

        float damage = baseDamage * (trackedVelocity / minVelocityForDamage);

        // Uses the IDamageable interface declared at the top of this file
        if (collision.gameObject.TryGetComponent<IDamageable>(out IDamageable damageable))
        {
            damageable.TakeDamage(damage);
            OnSuccessfulHit(collision);
        }
    }

    private void OnSuccessfulHit(Collision collision)
    {
        if (hitSound != null && audioSource != null)
            audioSource.PlayOneShot(hitSound);

        if (grabInteractable.interactorsSelecting.Count > 0)
        {
            var interactor = grabInteractable.interactorsSelecting[0];
            if (interactor is UnityEngine.XR.Interaction.Toolkit.Interactors.XRBaseInputInteractor controllerInteractor)
                controllerInteractor.SendHapticImpulse(hitHapticIntensity, hitHapticDuration);
        }

        SpawnHitEffect(collision.contacts[0].point, collision.contacts[0].normal);
    }

    private void PlaySwingSound()
    {
        if (swingSound != null && audioSource != null)
            audioSource.PlayOneShot(swingSound, 0.3f);
    }

    private void SpawnHitEffect(Vector3 position, Vector3 normal)
    {
        // Instantiate(hitParticles, position, Quaternion.LookRotation(normal));
    }

    private void OnDrawGizmos()
    {
        if (!Application.isPlaying) return;
        Gizmos.color = isGrabbed ? Color.green : Color.red;
        Gizmos.DrawWireSphere(transform.position, 0.1f);
    }
}