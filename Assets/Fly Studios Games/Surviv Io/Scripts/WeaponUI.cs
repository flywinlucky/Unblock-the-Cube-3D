using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class WeaponUI : MonoBehaviour
{
	[Header("References")]
	public WeaponControler weaponController; // assign in Inspector (sau Find)
	public Text weaponNameText;
	public Text ammoText; // ex: "6 / 24"

	[Header("Update")]
	[Tooltip("Cât de des actualizăm UI (s) pentru a nu pune presiune la Update()")]
	public float refreshInterval = 0.15f;

	private Coroutine _updateRoutine;

	private void Reset()
	{
		if (weaponController == null)
			weaponController = FindObjectOfType<WeaponControler>();

		if (weaponNameText == null || ammoText == null)
		{
			var texts = GetComponentsInChildren<Text>(true);
			if (texts.Length > 0 && weaponNameText == null) weaponNameText = texts[0];
			if (texts.Length > 1 && ammoText == null) ammoText = texts.Length > 1 ? texts[1] : ammoText;
		}
	}

	private void OnEnable()
	{
		_updateRoutine = StartCoroutine(RefreshLoop());
	}

	private void OnDisable()
	{
		if (_updateRoutine != null) StopCoroutine(_updateRoutine);
	}

	private IEnumerator RefreshLoop()
	{
		while (true)
		{
			RefreshUI();
			yield return new WaitForSeconds(refreshInterval);
		}
	}

	private void RefreshUI()
	{
		if (weaponController == null) return;

		var w = weaponController.CurrentWeapon;
		if (weaponNameText != null)
		{
			weaponNameText.text = (w != null) ? w.weaponName : "No Weapon";
		}

		if (ammoText != null)
		{
			// folosim API WeaponControler.GetAmmoInfo()
			var info = weaponController.GetAmmoInfo();
			int mag = info.mag;
			int reserve = info.reserve;
			int magSize = info.magSize;
			ammoText.text = $"{mag} / {reserve}"; // poți afișa și "/{magSize}" dacă dorești
		}
	}
}
