using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DestroyableEntity : MonoBehaviour
{
	[Header("Health")]
	public float maxHealth = 3f;
	private float _currentHealth;

	[Header("Hit Visual")]
	[Tooltip("Procentaj (0-1) cu cât se micșorează scale-ul curent la fiecare hit (ex: 0.2 = -20%).")]
	[Range(0f, 0.9f)]
	public float scaleReduction = 0.2f;

	[Tooltip("Scale minim permis pentru a evita valori foarte mici sau negative.")]
	public float minScale = 0.05f;

	void Start()
	{
		_currentHealth = maxHealth;
	}

	// Called when a Bullet hits this entity
	public virtual void OnHitByBullet(Bullet bullet)
	{
		if (bullet == null) return;
		TakeDamage(bullet.GetDamage());
	}

	// Aplica damage și micșorează scale-ul curent multiplicativ; distruge când health <= 0
	public void TakeDamage(float amount)
	{
		if (amount <= 0f) return;

		_currentHealth -= amount;
		_currentHealth = Mathf.Max(0f, _currentHealth);

		// Aplicăm reducerea scale-ului în funcție de scaleReduction pornind de la scale-ul curent
		float factor = Mathf.Clamp01(1f - scaleReduction);
		Vector3 newScale = transform.localScale * factor;

		// Clamp pe fiecare componentă la minScale
		newScale.x = Mathf.Max(newScale.x, minScale);
		newScale.y = Mathf.Max(newScale.y, minScale);
		newScale.z = Mathf.Max(newScale.z, minScale);

		transform.localScale = newScale;

		if (_currentHealth <= 0f)
		{
			Destroy(gameObject);
		}
	}
}
