using UnityEngine;

public class MainMenuCameraMouseLook : MonoBehaviour
{
    [Header("Rotation Settings")]
    public float rotationAmount = 5f; // Max degrees the camera will rotate
    public float smoothSpeed = 5f;

    private Quaternion initialRotation;

    void Start()
    {
        initialRotation = transform.localRotation;
    }

    void Update()
    {
        Vector2 mousePos = Input.mousePosition;
        Vector2 screenCenter = new Vector2(Screen.width / 2f, Screen.height / 2f);

        // Normalize mouse position relative to screen center (-1 to 1)
        Vector2 offset = (mousePos - screenCenter) / screenCenter;
        offset = Vector2.ClampMagnitude(offset, 1f);

        // Calculate target rotation angles
        float rotX = -offset.y * rotationAmount; // invert Y so up is up
        float rotY = offset.x * rotationAmount;

        Quaternion targetRotation = Quaternion.Euler(rotX, rotY, 0f);
        transform.localRotation = Quaternion.Lerp(transform.localRotation, initialRotation * targetRotation, Time.deltaTime * smoothSpeed);
    }
}
