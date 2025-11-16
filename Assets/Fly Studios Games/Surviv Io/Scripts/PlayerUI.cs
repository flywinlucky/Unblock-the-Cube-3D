using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class PlayerUI : MonoBehaviour
{
	[Header("References")]
	public PlayerHealth playerHealth; // assign în Inspector (sau găsește în child)
	public Slider healthSlider;       // slider UI (value 0..1)
	public Slider armorSlider;        // slider pentru armor
	[Space]
	public EquipmentIventorySlot helment_Slot;
	public EquipmentIventorySlot vest_Slot;

	
	private void Reset()
	{
		// fallback: încercăm să găsim automat dacă nu sunt setate
		if (playerHealth == null)
			playerHealth = FindObjectOfType<PlayerHealth>();

		if (healthSlider == null || armorSlider == null)
		{
			var sliders = GetComponentsInChildren<Slider>();
			if (sliders.Length > 0 && healthSlider == null) healthSlider = sliders[0];
			if (sliders.Length > 1 && armorSlider == null) armorSlider = sliders[1];
		}
	}

	private void OnEnable()
	{
		// asigurăm referințele imediat
		Reset();

		if (playerHealth != null)
		{
			// abonăm la event pentru actualizări ulterioare
			playerHealth.OnStatsChanged += UpdateUI;

			// inizializare sigură a slider-elor
			UpdateUI(playerHealth.CurrentHealth, playerHealth.MaxHealth, playerHealth.CurrentArmor, playerHealth.MaxArmor);
		}
	}

	private void OnDisable()
	{
		if (playerHealth != null)
			playerHealth.OnStatsChanged -= UpdateUI;
	}

	private void UpdateUI(float currentHealth, float maxHealth, float currentArmor, float maxArmor)
	{
		if (healthSlider != null)
		{
			healthSlider.maxValue = maxHealth > 0f ? maxHealth : healthSlider.maxValue;
			healthSlider.value = Mathf.Clamp(currentHealth, 0f, maxHealth);
		}

		if (armorSlider != null)
		{
			armorSlider.maxValue = maxArmor > 0f ? maxArmor : armorSlider.maxValue;
			armorSlider.value = Mathf.Clamp(currentArmor, 0f, maxArmor);
		}
	}
}
