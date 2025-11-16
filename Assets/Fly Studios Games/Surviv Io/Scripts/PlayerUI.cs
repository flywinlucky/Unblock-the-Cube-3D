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
			// abonăm la event pentru actualizări ulterioare
			playerHealth.OnHealthChanged += UpdateHealthUI;

			// inizializare sigură a slider-ului (așteptăm un frame ca UI să fie gata)
			StartCoroutine(EnsureSliderInitialized());
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

	private IEnumerator EnsureSliderInitialized()
	{
		// așteptăm sfârșitul frame-ului pentru a ne asigura că UI e construit
		yield return new WaitForEndOfFrame();

		if (playerHealth == null || healthSlider == null) yield break;

		// Forțăm update-ul layout-ului UI înainte de a seta valorile
		Canvas.ForceUpdateCanvases();

		healthSlider.maxValue = playerHealth.MaxHealth > 0f ? playerHealth.MaxHealth : 1f;
		healthSlider.value = Mathf.Clamp(playerHealth.CurrentHealth, 0f, playerHealth.MaxHealth);
	}
}
