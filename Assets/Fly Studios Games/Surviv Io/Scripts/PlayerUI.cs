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
	[Header("F To Select UI")]
	public GameObject FtoSellect;     // container UI prompt
	public Text fToSelect_Text;       // text unde afișăm numele itemului

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
		// asigurăm promptul ascuns la activare
		if (FtoSellect != null) FtoSellect.SetActive(false);
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

	public void ApplyEquipment(EquipmentData equipmentData)
	{
		if (equipmentData == null) return;

		if (equipmentData.equipmentType == EquipmentData.EquipmentType.Helmets)
		{
			helment_Slot.RefreshEquipamentSlot(equipmentData.equipmentSpriteIcon, equipmentData.equipmentLevel);
		}
		else if (equipmentData.equipmentType == EquipmentData.EquipmentType.Vests)
		{
			vest_Slot.RefreshEquipamentSlot(equipmentData.equipmentSpriteIcon, equipmentData.equipmentLevel);
		}
	}

	// Helpers pentru promptul "F to Select"
	public void ShowFtoSellect(string itemName)
	{
		if (fToSelect_Text != null) fToSelect_Text.text = itemName;
		if (FtoSellect != null) FtoSellect.SetActive(true);
	}
	public void HideFtoSellect()
	{
		if (FtoSellect != null) FtoSellect.SetActive(false);
	}
}
