using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// This script makes the GameObject it's attached to always face the main camera.
/// </summary>
public class Billboard : MonoBehaviour
{
    private Transform mainCameraTransform;

    private void Start()
    {
        if (Camera.main != null)
        {
            mainCameraTransform = Camera.main.transform;
        }
    }

    private void LateUpdate()
    {
        if (mainCameraTransform == null)
        {
            // Try to find the camera again if it wasn't available at Start
            if (Camera.main != null) mainCameraTransform = Camera.main.transform;
            else return;
        }

        // Rotate the object to face the camera
        transform.LookAt(transform.position + mainCameraTransform.rotation * Vector3.forward,
                         mainCameraTransform.rotation * Vector3.up);
    }
} 