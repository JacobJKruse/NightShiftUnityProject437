using UnityEngine;

public class PlayerCam : MonoBehaviour
{
    public float sensX;
    public float sensY;
    public Transform orientation;

    private float xRotation;
    private float yRotation;
    private Light spotlight; // Reference to the child spotlight
    private bool isLightOn = true; // Track light state

    private void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        // Get the Spotlight child object 
        spotlight = GetComponentInChildren<Light>();

        if (spotlight != null)
        {
            spotlight.enabled = isLightOn; // Set initial state
        }
        else
        {
            Debug.LogWarning("No spotlight found! Make sure it's a child object.");
        }
    }

    private void Update()
    {
        // Get mouse input
        float mouseX = Input.GetAxisRaw("Mouse X") * Time.deltaTime * sensX;
        float mouseY = Input.GetAxisRaw("Mouse Y") * Time.deltaTime * sensY;

        yRotation += mouseX;
        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -90f, 90f);

        // Rotate cam and orientation
        transform.rotation = Quaternion.Euler(xRotation, yRotation, 0);
        orientation.rotation = Quaternion.Euler(0, yRotation, 0);

        // Toggle light on/off when "F" is pressed
        if (Input.GetKeyDown(KeyCode.F) && spotlight != null)
        {
            isLightOn = !isLightOn; // Toggle state
            spotlight.enabled = isLightOn; // Apply state
        }
    }
}
