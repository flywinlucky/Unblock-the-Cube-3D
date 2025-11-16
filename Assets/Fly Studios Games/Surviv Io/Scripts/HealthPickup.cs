using UnityEngine;

public class HealthPickup : MonoBehaviour
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

		ph.Heal(amount);

		// efecte vizuale / sunet pot fi adăugate aici
		Destroy(gameObject);
	}
}