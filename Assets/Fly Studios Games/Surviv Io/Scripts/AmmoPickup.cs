using UnityEngine;

public class AmmoPickup : MonoBehaviour
{
	[Tooltip("Câtă muniție dă acest pickup.")]
	public int ammoAmount = 6;

	[Tooltip("Dacă este setat, se va adăuga doar la arma corespunzătoare (opțional).")]
	public WeaponData forWeapon;

	// Trigger 2D
	private void OnTriggerEnter2D(Collider2D other)
	{
		TryGiveAmmo(other.gameObject);
	}

	// Trigger 3D
	private void OnTriggerEnter(Collider other)
	{
		TryGiveAmmo(other.gameObject);
	}

	private void TryGiveAmmo(GameObject other)
	{
		if (other == null) return;

		// căutăm WeaponControler pe jucător (sau pe obiectul care ridică pickup-ul)
		var wc = other.GetComponentInChildren<WeaponControler>();
		if (wc == null) return;

		// dacă pickup-ul este legat de o anumită armă, verificăm
		if (forWeapon != null && wc.CurrentWeapon != forWeapon) return;

		int added = wc.AddAmmo(ammoAmount);
		if (added > 0)
		{
			// poți adăuga efecte vizuale / sunet aici
			Destroy(this.gameObject);
		}
	}
}
