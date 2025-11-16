using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class PlayerUI : MonoBehaviour
{
	[Header("References")]
	public PlayerHealth playerHealth; // assign în Inspector (sau găsește în child)
	public Slider healthSlider;       // slider UI (value 0..1)
	
	private void Reset()
	{
		// fallback: încercăm să găsim automat dacă nu sunt setate
		if (playerHealth == null)
			playerHealth = FindObjectOfType<PlayerHealth>();

		if (healthSlider == null)
		{
			// cautăm un Slider în copii
			healthSlider = GetComponentInChildren<Slider>();
		}
	}

	private void OnEnable()
	{
		// asigurăm referințele imediat
		Reset();

		if (playerHealth != null)
		{
			// setăm maxValue și valoarea inițială a sliderului
			if (healthSlider != null)
			{
				healthSlider.maxValue = playerHealth.MaxHealth > 0f ? playerHealth.MaxHealth : 1f;
				healthSlider.value = Mathf.Clamp(playerHealth.CurrentHealth, 0f, playerHealth.MaxHealth);
			}

			// abonăm la event pentru actualizări ulterioare
			playerHealth.OnHealthChanged += UpdateHealthUI;
		}
	}

	private void OnDisable()
	{
		if (playerHealth != null)
			playerHealth.OnHealthChanged -= UpdateHealthUI;
	}

	private void UpdateHealthUI(float current, float max)
	{
		if (healthSlider != null)
		{
			// asigurăm maxValue actualizat (în caz că se schimbă în runtime)
			healthSlider.maxValue = max > 0f ? max : healthSlider.maxValue;
			healthSlider.value = Mathf.Clamp(current, 0f, max);
		}
	}
}
