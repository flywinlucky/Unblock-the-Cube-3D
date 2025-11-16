using UnityEngine;

public class HealthPickup : MonoBehaviour
{
	[Tooltip("Câtă viață adaugă acest pickup.")]
	public float healAmount = 3f;

	private void OnTriggerEnter2D(Collider2D other)
	{
		TryHeal(other.gameObject);
	}

	private void OnTriggerEnter(Collider other)
	{
		TryHeal(other.gameObject);
	}

	private void TryHeal(GameObject other)
	{
		if (other == null) return;
		var ph = other.GetComponentInChildren<PlayerHealth>();
		if (ph == null) return;

		ph.Heal(healAmount);
		// efecte vizuale / sunet pot fi adăugate aici
		Destroy(gameObject);
	}
}
