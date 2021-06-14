using UnityEngine;

public class CollisionEvents : MonoBehaviour
{
    public System.Action<Collision> onCollisionEnter;
    public System.Action<Collision> onCollisionStay;
    public System.Action<Collision> onCollisionExit;
    public System.Action<Collider> onTriggerEnter;
    public System.Action<Collider> onTriggerStay;
    public System.Action<Collider> onTriggerExit;

    void OnCollisionEnter(Collision collision)
    {
        onCollisionEnter?.Invoke(collision);
    }

    void OnCollisionStay(Collision collision)
    {
        onCollisionStay?.Invoke(collision);
    }

    void OnCollisionExit(Collision collision)
    {
        onCollisionExit?.Invoke(collision);
    }

    void OnTriggerEnter(Collider other)
    {
        onTriggerEnter?.Invoke(other);
    }

    void OnTriggerStay(Collider other)
    {
        onTriggerStay?.Invoke(other);
    }

    void OnTriggerExit(Collider other)
    {
        onTriggerExit?.Invoke(other);
    }
}
