using UnityEngine;

public class YAxisBillboard : MonoBehaviour {
    void LateUpdate() {
        if (Camera.main == null) return;

        Vector3 cameraPos = Camera.main.transform.position;

        // Get direction to camera on XZ plane
        Vector3 lookDirection = new Vector3(
            cameraPos.x - transform.position.x,
            0,
            cameraPos.z - transform.position.z
        );

        // Only rotate if direction is significant
        if (lookDirection.sqrMagnitude > 0.001f) {
            Quaternion targetRotation = Quaternion.LookRotation(lookDirection);
            transform.rotation = targetRotation;
        }
    }
}