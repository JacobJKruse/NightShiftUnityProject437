using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerPickupDrop : MonoBehaviour
{
    [SerializeField] private Transform playerCameraTransform;
    [SerializeField] private LayerMask playerLayerMask;
    [SerializeField] private Transform objGrabTransform;
    [SerializeField] private Transform orientation;
    [SerializeField] private Light spotlight; // Reference to the flashlight

    private ObjectGrabber grabber;
    private PlayerControl playerControl;

    void Start()
    {
        playerControl = GetComponent<PlayerControl>();

        if (spotlight == null)
        {
            spotlight = GetComponentInChildren<Light>(); // Find flashlight if not assigned
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.E))
        {
            HandlePickupDrop();
        }

        if (spotlight != null && spotlight.enabled) // Check if flashlight is ON
        {
            DetectEnemyWithLight();
        }
    }

    void HandlePickupDrop()
    {
        if (grabber == null)
        {
            float pickupDistance = 2f;

            if (Physics.Raycast(playerCameraTransform.position, playerCameraTransform.forward, out RaycastHit hitInfo, pickupDistance, playerLayerMask))
            {
                if (hitInfo.transform.TryGetComponent(out ObjectToken token))
                {
                    if (playerControl != null)
                    {
                        playerControl.tokens = Mathf.Min(24, playerControl.tokens + token.tokenValue);
                        Destroy(hitInfo.transform.gameObject);
                        Debug.Log("Token Collected: +" + token.tokenValue);
                    }
                }
                else if (hitInfo.transform.TryGetComponent(out grabber))
                {
                    grabber.Grab(objGrabTransform);
                    Debug.Log("Grab");
                }
            }
        }
        else
        {
            grabber.Drop();
            grabber = null;
        }

        HandleDoorElevatorInteraction();
    }

    void HandleDoorElevatorInteraction()
    {
        if (Physics.Raycast(playerCameraTransform.position, playerCameraTransform.TransformDirection(Vector3.forward), out RaycastHit hit))
        {
            if (hit.transform.CompareTag("door"))
            {
                hit.transform.gameObject.GetComponent<Door>()?.ActionDoor();
            }

            string name = hit.collider.gameObject.name;

            if (name.StartsWith("Button floor"))
            {
                hit.transform.gameObject.GetComponent<pass_on_parent>().MyParent
                    .GetComponent<evelator_controll>()
                    .AddTaskEve(name);
            }
        }
    }

    void DetectEnemyWithLight()
    {
        float detectionRange = 10f; 
        Vector3 rayStart = playerCameraTransform.position;
        Vector3 rayDirection = playerCameraTransform.forward;

        
        Debug.DrawRay(rayStart, rayDirection * detectionRange, Color.red, 0.1f);

        if (Physics.Raycast(rayStart, rayDirection, out RaycastHit hit, detectionRange))
        {
            Debug.Log($"Raycast hit: {hit.collider.gameObject.name} (Tag: {hit.collider.tag})");

            if (hit.collider.CompareTag("Enemy")) // Using "Enemy" tag as requested
            {
                // Try to get any of the three NavMesh components
                NavMesh1 enemyAI1 = hit.collider.GetComponent<NavMesh1>();
                NavMesh2 enemyAI2 = hit.collider.GetComponent<NavMesh2>();
                NavMesh3 enemyAI3 = hit.collider.GetComponent<NavMesh3>();

                float lightIntensity = spotlight != null ? spotlight.intensity : 0f; // Get flashlight intensity

                // Call FleeFromLight on whichever component exists
                if (enemyAI1 != null)
                {
                    enemyAI1.FleeFromLight(playerCameraTransform.position);
                    Debug.Log("NavMesh1 enemy detected! Triggering escape...");
                }
                else if (enemyAI2 != null)
                {
                    enemyAI2.FleeFromLight(playerCameraTransform.position);
                    Debug.Log("NavMesh2 enemy detected! Triggering escape...");
                }
                else if (enemyAI3 != null)
                {
                    enemyAI3.FleeFromLight(playerCameraTransform.position);
                    Debug.Log("NavMesh3 enemy detected! Triggering escape...");
                }
            }
        }
        else
        {
            Debug.Log("Raycast did not hit anything relevant.");
        }
    }


}
