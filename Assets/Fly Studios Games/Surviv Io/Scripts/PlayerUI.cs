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
	[Header("Reloading")]
	public GameObject FreloadingUI;   
	public Text reloadingTimer_Text;

	// Optional: referință la WeaponControler (dacă nu e setată, o găsim automat)
	public WeaponControler weaponController;

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
		if (weaponController == null)
			weaponController = FindObjectOfType<WeaponControler>();
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

		// subscribe la event-uri de reload
		if (weaponController == null)
			weaponController = FindObjectOfType<WeaponControler>();
		if (weaponController != null)
		{
			weaponController.OnReloadStarted -= HandleReloadStarted;
			weaponController.OnReloadStarted += HandleReloadStarted;
			weaponController.OnReloadProgress -= HandleReloadProgress;
			weaponController.OnReloadProgress += HandleReloadProgress;
			weaponController.OnReloadFinished -= HandleReloadFinished;
			weaponController.OnReloadFinished += HandleReloadFinished;
		}

		// ascunde UI de reloading la start
		if (FreloadingUI != null) FreloadingUI.SetActive(false);
		if (reloadingTimer_Text != null) reloadingTimer_Text.text = "";
	}

	private void OnDisable()
	{
		if (playerHealth != null)
			playerHealth.OnStatsChanged -= UpdateUI;
		if (weaponController != null)
		{
			weaponController.OnReloadStarted -= HandleReloadStarted;
			weaponController.OnReloadProgress -= HandleReloadProgress;
			weaponController.OnReloadFinished -= HandleReloadFinished;
		}
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

	private void HandleReloadStarted(float duration)
	{
		if (FreloadingUI != null) FreloadingUI.SetActive(duration > 0f);
		if (reloadingTimer_Text != null)
			reloadingTimer_Text.text = duration > 0f ? duration.ToString("0.00") : "";
	}

	private void HandleReloadProgress(float remaining)
	{
		if (FreloadingUI == null || reloadingTimer_Text == null) return;
		if (remaining > 0f)
		{
			FreloadingUI.SetActive(true);
			reloadingTimer_Text.text = remaining.ToString("0.00");
		}
	}

	private void HandleReloadFinished()
	{
		if (FreloadingUI != null) FreloadingUI.SetActive(false);
		if (reloadingTimer_Text != null) reloadingTimer_Text.text = "";
	}
}
