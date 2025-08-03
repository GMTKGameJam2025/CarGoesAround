using UnityEngine;
using PrimeTween;

public class FloatingCursor : MonoBehaviour
{
    [Header("Bobbing Animation Settings")]
    [SerializeField] private float bobbingHeight = 0.5f;
    [SerializeField] private float bobbingDuration = 1.5f;
    [SerializeField] private Ease bobbingEase = Ease.InOutSine;
    
    [Header("Rotation")]
    [SerializeField] private bool enableRotation = false;
    [SerializeField] private float rotationSpeed = 45f; // degrees per second
    
    [Header("Scaling Pulse")]
    [SerializeField] private bool enableScalePulse = false;
    [SerializeField] private float pulseScale = 1.2f;
    [SerializeField] private float pulseDuration = 0.8f;
    
    private Vector3 originalPosition;
    private Vector3 originalScale;
    private Tween bobbingTween;
    private Tween rotationTween;
    private Tween scaleTween;

    public void StartAnimation()
    {
        // Store original values
        originalPosition = transform.position;
        originalScale = transform.localScale;
        
        // Start the animations
        StartBobbingAnimation();
        
        if (enableRotation)
        {
            StartRotationAnimation();
        }
        
        if (enableScalePulse)
        {
            StartScalePulseAnimation();
        }
    }
    
    private void StartBobbingAnimation()
    {
        // Create a bobbing animation that moves up and down continuously
        Vector3 targetPosition = originalPosition + Vector3.up * bobbingHeight;

        if (bobbingTween.isAlive)
        {
            bobbingTween.Stop();
        }

        bobbingTween = Tween.Position(transform, targetPosition, bobbingDuration, bobbingEase, cycles: -1, CycleMode.Yoyo); // Infinite loops with yoyo (back and forth)
    }
    
    private void StartRotationAnimation()
    {
        if (rotationTween.isAlive)
        {
            rotationTween.Stop();
        }
        
        // Continuous slow rotation around Y-axis
        rotationTween = Tween.Rotation(transform,
            transform.rotation * Quaternion.Euler(0, 360f, 0),
            360f / rotationSpeed,
            Ease.Linear, cycles: -1); // Infinite loops, restarting each time
    }
    
    private void StartScalePulseAnimation()
    {
        if (scaleTween.isAlive)
        {
            scaleTween.Stop();
        }
        
        // Gentle scale pulsing
        Vector3 targetScale = originalScale * pulseScale;

        scaleTween = Tween.Scale(transform, targetScale, pulseDuration, Ease.InOutQuad, cycles: -1, CycleMode.Yoyo);
    }
    
    public void SetBobbingIntensity(float intensity)
    {
        // Stop current bobbing
        if (bobbingTween.isAlive)
        {
            bobbingTween.Stop();
        }
        
        // Update bobbing height
        bobbingHeight = intensity;
        
        // Restart with new intensity
        StartBobbingAnimation();
    }
    
    public void SetBobbingSpeed(float speed)
    {
        // Stop current bobbing
        if (bobbingTween.isAlive)
        {
            bobbingTween.Stop();
        }
        
        // Update duration (lower duration = faster)
        bobbingDuration = 1f / speed;
        
        // Restart with new speed
        StartBobbingAnimation();
    }
    
    public void StopAllAnimations()
    {
        if (bobbingTween.isAlive) bobbingTween.Stop();
        if (rotationTween.isAlive) rotationTween.Stop();
        if (scaleTween.isAlive) scaleTween.Stop();
        
        // Reset to original values
        transform.position = originalPosition;
        transform.localScale = originalScale;
    }
    
    private void OnDestroy()
    {
        // Clean up tweens when object is destroyed
        if (bobbingTween.isAlive) bobbingTween.Stop();
        if (rotationTween.isAlive) rotationTween.Stop();
        if (scaleTween.isAlive) scaleTween.Stop();
    }
    
    // For integration with your grid system
    public void UpdateCursorPosition(Vector3 newPosition)
    {
        // Update the original position so bobbing continues from new location
        originalPosition = newPosition;
        
        // If we want to smoothly move to the new position while maintaining bobbing
        if (bobbingTween.isAlive)
        {
            bobbingTween.Stop();
        }
        
        // Move to new position smoothly, then restart bobbing
        Tween.Position(transform, newPosition, 0.1f, Ease.OutQuad)
            .OnComplete(StartBobbingAnimation);
    }
    
    // Alternative: Instant position update while maintaining bobbing offset
    public void UpdateCursorPositionInstant(Vector3 newPosition)
    {
        Vector3 currentOffset = transform.position - originalPosition;
        originalPosition = newPosition;
        transform.position = originalPosition + currentOffset;
    }
}