using UnityEngine;

public class CameraRotate : MonoBehaviour
{
    public float rotationSpeed = 10f; // Adjust speed as needed

    void Update()
    {
        transform.Rotate(-rotationSpeed * Time.deltaTime, 0,rotationSpeed * Time.deltaTime);
    }
}
