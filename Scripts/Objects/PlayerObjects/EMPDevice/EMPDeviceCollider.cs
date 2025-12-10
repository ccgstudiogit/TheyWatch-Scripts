using UnityEngine;

public class EMPDeviceCollider : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent(out Stunnable stunnable))
        {
            stunnable.Stun();
        }
    }
}
