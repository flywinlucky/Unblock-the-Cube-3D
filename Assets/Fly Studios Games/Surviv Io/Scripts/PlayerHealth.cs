using System;
using Sirenix.OdinInspector;
using UnityEngine;

public class PlayerHealth : MonoBehaviour
{
	[Header("Health")]
	public float maxHealth;
	private float _currentHealth;

	[Header("Armor")]
	public float maxArmor;
	private float _currentArmor;

	private float _damageReductionPercentage = 0f; // Current damage reduction percentage

	// Event invocat când health sau armor se schimbă: (currentHealth, maxHealth, currentArmor, maxArmor)
	public event Action<float, float, float, float> OnStatsChanged;

	public float CurrentHealth => _currentHealth;
	public float MaxHealth => maxHealth;
	public float CurrentArmor => _currentArmor;
	public float MaxArmor => maxArmor;

	private void Awake()
	{
		// Initialize health and armor to their maximum values
		_currentHealth = Mathf.Clamp(maxHealth, 0f, 100f);
		_currentArmor = Mathf.Clamp(maxArmor, 0f, 100f);

		// Trigger the stats changed event to update any listeners (e.g., UI)
		OnStatsChanged?.Invoke(_currentHealth, maxHealth, _currentArmor, maxArmor);
	}

	// Apply damage reduction from equipment
	public void ApplyDamageReduction(float reductionPercentage)
	{
		_damageReductionPercentage = Mathf.Clamp(reductionPercentage, 0f, 100f);
	}

	[Button]
	public void TakeDamage(float amount)
	{
		if (amount <= 0f) return;

		// Reduce damage based on the current damage reduction percentage
		float reducedDamage = amount * (1f - _damageReductionPercentage / 100f);

		// Damage is first absorbed by armor
		if (_currentArmor > 0f)
		{
			float remainingDamage = Mathf.Max(0f, reducedDamage - _currentArmor);
			_currentArmor = Mathf.Max(0f, _currentArmor - reducedDamage);
			reducedDamage = remainingDamage;
		}

		// Remaining damage is applied to health
		if (reducedDamage > 0f)
		{
			_currentHealth = Mathf.Max(0f, _currentHealth - reducedDamage);
		}

		OnStatsChanged?.Invoke(_currentHealth, maxHealth, _currentArmor, maxArmor);

		if (_currentHealth <= 0f)
		{
			// gestionare moarte (poți extinde)
			Destroy(gameObject);
		}
	}

	[Button]
	public void Heal(float amount)
	{
		if (amount <= 0f) return;
		_currentHealth = Mathf.Min(maxHealth, _currentHealth + amount);
		OnStatsChanged?.Invoke(_currentHealth, maxHealth, _currentArmor, maxArmor);
	}

	[Button]
	public void AddArmor(float amount)
	{
		if (amount <= 0f) return;
		_currentArmor = Mathf.Min(maxArmor, _currentArmor + amount);
		OnStatsChanged?.Invoke(_currentHealth, maxHealth, _currentArmor, maxArmor);
	}

	// Utility: set health and armor directly (ex: debug)
	[Button]
	public void SetStats(float health, float armor)
	{
		_currentHealth = Mathf.Clamp(health, 0f, maxHealth);
		_currentArmor = Mathf.Clamp(armor, 0f, maxArmor);
		OnStatsChanged?.Invoke(_currentHealth, maxHealth, _currentArmor, maxArmor);
	}
}
