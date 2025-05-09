using UnityEngine;

public class ObjectGrabber : MonoBehaviour
{
    private Rigidbody objRigidbody;
    private Transform objGrabPoint;

    private void Awake()
    {
        objRigidbody = GetComponent<Rigidbody>();
    }

    public void Grab(Transform objGrabPointTransform) { 
        this.objGrabPoint = objGrabPointTransform;
        objRigidbody.useGravity = false;
    }

    public void Drop() { 
        this.objGrabPoint = null;
        objRigidbody.useGravity = true;
    }

    private void Update()
    {
        if (objGrabPoint != null)
        {
            float lerpSpeed = 10f;
            Vector3 newPos = Vector3.Lerp(transform.position, objGrabPoint.position, Time.deltaTime * lerpSpeed);
            objRigidbody.MovePosition(newPos);
        }
    }
}
