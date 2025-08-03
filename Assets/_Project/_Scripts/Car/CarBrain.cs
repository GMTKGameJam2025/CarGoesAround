using UnityEngine;

[SelectionBase]
public class CarBrain : MonoBehaviour
{
    public CarMovement movement;

    [Header("Audio Settings")]
    public AudioClip engineSound;
    public float minPitch = 0.8f;
    public float maxPitch = 1.5f;
    public float minVolume = 0.3f;
    public float maxVolume = 0.8f;

    private AudioSource audioSource;
    private bool wasMovingLastFrame = false;

    void Start()
    {
        // Get or create AudioSource component
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }

        // Configure AudioSource for engine sound
        if (engineSound != null)
        {
            audioSource.clip = engineSound;
            audioSource.loop = true;
            audioSource.volume = 0f;
            audioSource.pitch = minPitch;
            audioSource.Play();
        }
    }

    void Update()
    {
        if (movement == null || audioSource == null || engineSound == null) return;

        // Check if car is moving and not destroyed
        bool isMoving = IsCarMoving() && !movement.IsDestroyed();

        if (isMoving)
        {
            // Start engine sound if it wasn't playing
            if (!wasMovingLastFrame)
            {
                if (!audioSource.isPlaying)
                {
                    audioSource.Play();
                }
            }

            // Adjust pitch and volume based on speed
            float speedFactor = GetSpeedFactor();
            audioSource.pitch = Mathf.Lerp(minPitch, maxPitch, speedFactor);
            audioSource.volume = Mathf.Lerp(minVolume, maxVolume, speedFactor);
        }
        else
        {
            // Fade out engine sound when not moving
            audioSource.volume = Mathf.Lerp(audioSource.volume, 0f, Time.deltaTime * 3f);

            // Stop audio if volume is very low
            if (audioSource.volume < 0.01f && audioSource.isPlaying)
            {
                audioSource.Stop();
            }
        }

        wasMovingLastFrame = isMoving;
    }

    private bool IsCarMoving()
    {
        if (movement == null) return false;

        // Check if car has a rigidbody and is moving
        Rigidbody rb = movement.GetComponent<Rigidbody>();
        if (rb != null)
        {
            return rb.linearVelocity.magnitude > 0.1f;
        }

        return false;
    }

    private float GetSpeedFactor()
    {
        if (movement == null) return 0f;

        Rigidbody rb = movement.GetComponent<Rigidbody>();
        if (rb != null)
        {
            float currentSpeed = rb.linearVelocity.magnitude;
            float maxSpeed = movement.moveSpeed; // Use the moveSpeed from CarMovement
            return Mathf.Clamp01(currentSpeed / maxSpeed);
        }

        return 0f;
    }
}