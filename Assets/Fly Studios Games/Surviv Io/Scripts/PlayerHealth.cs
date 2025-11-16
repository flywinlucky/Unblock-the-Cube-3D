using System;
using Sirenix.OdinInspector;
using UnityEngine;

public class PlayerHealth : MonoBehaviour
{
	[Header("Health")]
	public float maxHealth;
	private float _currentHealth;

	// Event invocat când health se schimbă: (current, max)
	public event Action<float, float> OnHealthChanged;

	public float CurrentHealth => _currentHealth;
	public float MaxHealth => maxHealth;

	private void Awake()
	{
		_currentHealth = maxHealth;
	}

[Button]
	public void TakeDamage(float amount)
	{
		if (amount <= 0f) return;
		_currentHealth = Mathf.Max(0f, _currentHealth - amount);
		OnHealthChanged?.Invoke(_currentHealth, maxHealth);
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
		OnHealthChanged?.Invoke(_currentHealth, maxHealth);
	}

	// Utility: set health direct (ex: debug)
	[Button]
	public void SetHealth(float value)
	{
		_currentHealth = Mathf.Clamp(value, 0f, maxHealth);
		OnHealthChanged?.Invoke(_currentHealth, maxHealth);
	}
}
