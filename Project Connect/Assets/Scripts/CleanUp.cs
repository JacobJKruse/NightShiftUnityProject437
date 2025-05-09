using UnityEngine;
using System.Collections.Generic;

public class CleanUp : MonoBehaviour
{
    public GameObject[] trackedObjects; // Assign in the Inspector

    private HashSet<GameObject> objectsInTrigger = new HashSet<GameObject>();
    private bool cleanupDone = false;

    private void Start()
    {
        Debug.Log("CleanUp script started. Tracked objects:");
        foreach (var obj in trackedObjects)
        {
            if (obj != null)
                Debug.Log($" - {obj.name}");
            else
                Debug.LogWarning("One of the tracked objects is null!");
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log($"Trigger entered by: {other.gameObject.name}");

        if (cleanupDone) return;

        foreach (GameObject obj in trackedObjects)
        {
            if (other.gameObject == obj)
            {
                Debug.Log($"Tracked object entered: {obj.name}");
                objectsInTrigger.Add(obj);
                break;
            }
        }

        CheckIfAllObjectsInside();
    }

    private void OnTriggerExit(Collider other)
    {
        Debug.Log($"Trigger exited by: {other.gameObject.name}");

        if (cleanupDone) return;

        if (objectsInTrigger.Contains(other.gameObject))
        {
            Debug.Log($"Tracked object exited: {other.gameObject.name}");
            objectsInTrigger.Remove(other.gameObject);
        }
    }

    private void CheckIfAllObjectsInside()
    {
        Debug.Log($"Objects currently inside: {objectsInTrigger.Count}/{trackedObjects.Length}");
        if (objectsInTrigger.Count == trackedObjects.Length)
        {
            Debug.Log("✅ All objects cleaned up");
            cleanupDone = true;

            // Optional: Do something else, like disable or destroy objects
        }
    }

    // Optional: Draw trigger bounds in Scene view for debugging
    private void OnDrawGizmos()
    {
        Collider triggerCollider = GetComponent<Collider>();
        if (triggerCollider != null && triggerCollider.isTrigger)
        {
            Gizmos.color = new Color(0, 1, 0, 0.25f); // translucent green
            Gizmos.matrix = transform.localToWorldMatrix;

            if (triggerCollider is BoxCollider box)
            {
                Gizmos.DrawCube(box.center, box.size);
            }
            else if (triggerCollider is SphereCollider sphere)
            {
                Gizmos.DrawSphere(sphere.center, sphere.radius);
            }
        }
    }
}
