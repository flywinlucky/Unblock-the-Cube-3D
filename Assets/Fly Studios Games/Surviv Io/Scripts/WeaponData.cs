using UnityEngine;

[CreateAssetMenu(fileName = "WeaponData", menuName = "SurvivIo/Weapon Data")]
public class WeaponData : ScriptableObject
{
	public string weaponName = "New Weapon";

	[Header("Projectile")]
	public GameObject bulletPrefab;
	public float bulletSpeed = 15f;
	public float damage = 1f;

	[Header("Fire")]
	public float fireRate = 0.18f;
	public bool automatic = true;

	[Header("Ammo")]
	public int magazineSize = 6;      // câte gloanțe într-un încărcător
	public int maxReserveAmmo = 30;   // cât rezervă maximă poți avea
	public int startingReserveAmmo = 12;

	[Header("Reload")]
	public float reloadTime = 0.8f; // (opțional, folosește dacă vrei coroutine)
}
