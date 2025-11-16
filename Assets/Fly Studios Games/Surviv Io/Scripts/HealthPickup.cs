using UnityEngine;

public class HealthPickup : MonoBehaviour
{
	public enum PickupType { Health, Armor }

	[Tooltip("Tipul pickup-ului: Health sau Armor.")]
	public PickupType pickupType = PickupType.Health;

	[Tooltip("Câtă viață sau armură adaugă acest pickup.")]
	public float amount = 3f;

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

		if (pickupType == PickupType.Health)
		{
			ph.Heal(amount);
		}
		else if (pickupType == PickupType.Armor)
		{
			ph.AddArmor(amount);
		}

		// efecte vizuale / sunet pot fi adăugate aici
		Destroy(gameObject);
	}
}
