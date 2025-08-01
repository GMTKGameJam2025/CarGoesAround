using UnityEngine;

public class MainMenuCameraMouseLook : MonoBehaviour
{
    [Header("Mouse Follow Settings")]
    public float sensitivity = 0.05f;     // How much the camera moves with the mouse
    public float maxOffset = 0.5f;        // Maximum movement from the center
    public float smoothSpeed = 5f;        // How smoothly it returns/stays

    private Vector3 initialPosition;

    void Start()
    {
        initialPosition = transform.localPosition;
    }

    void Update()
    {
        // Get mouse position in viewport coordinates (0 to 1)
        Vector2 mouseViewport = Camera.main.ScreenToViewportPoint(Input.mousePosition);

        // Offset from center (0.5, 0.5), range -0.5 to +0.5
        Vector2 offsetFromCenter = mouseViewport - new Vector2(0.5f, 0.5f);

        // Calculate desired offset
        Vector3 desiredOffset = new Vector3(
            offsetFromCenter.x * sensitivity,
            offsetFromCenter.y * sensitivity,
            0f
        );

        // Clamp movement to maxOffset
        desiredOffset = Vector3.ClampMagnitude(desiredOffset, maxOffset);

        // Smooth movement
        Vector3 targetPosition = initialPosition + desiredOffset;
        transform.localPosition = Vector3.Lerp(transform.localPosition, targetPosition, Time.deltaTime * smoothSpeed);
    }
}
