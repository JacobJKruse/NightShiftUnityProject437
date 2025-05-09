using UnityEngine;

public class EBoxTrigger : MonoBehaviour
{
    public eventHandel parentHandler;

    private void OnTriggerEnter(Collider other)
    {
        parentHandler.HandleTriggerEnter(other);
    }

    private void OnTriggerExit(Collider other)
    {
        parentHandler.HandleTriggerExit(other);
    }
}
