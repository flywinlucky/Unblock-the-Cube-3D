using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class PlayerUI : MonoBehaviour
{
	[Header("References")]
	public PlayerHealth playerHealth; // assign în Inspector (sau găsește în child)
	public Slider healthSlider;       // slider UI (value 0..1)
	public Text healthText;          // optional: "HP 8 / 10"

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

		if (healthText == null)
		{
			healthText = GetComponentInChildren<Text>();
		}
	}

	private void OnEnable()
	{
		if (playerHealth != null)
		{
			playerHealth.OnHealthChanged += UpdateHealthUI;
			// inițializare
			UpdateHealthUI(playerHealth.CurrentHealth, playerHealth.MaxHealth);
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
			float t = max > 0f ? current / max : 0f;
			healthSlider.value = Mathf.Clamp01(t);
		}

		if (healthText != null)
		{
			healthText.text = $"HP {Mathf.CeilToInt(current)} / {Mathf.CeilToInt(max)}";
		}
	}
}
