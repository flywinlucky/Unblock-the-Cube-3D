using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ArmorPickup : MonoBehaviour
{
    public float amount;

    private void OnTriggerEnter2D(Collider2D other)
    {
        TryApplyPickup(other.gameObject);
    }

    private void OnTriggerEnter(Collider other)
    {
        TryApplyPickup(other.gameObject);
    }

    private void TryApplyPickup(GameObject other)
    {
        if (other == null) return;
        var ph = other.GetComponentInChildren<PlayerHealth>();
        if (ph == null) return;

        ph.AddArmor(amount);

        // efecte vizuale / sunet pot fi adăugate aici
        Destroy(gameObject);
    }
}