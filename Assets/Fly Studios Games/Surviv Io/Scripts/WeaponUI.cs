using System.Collections;
using System.Collections.Generic;
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

    [Space]
    public List<WeaponIventorySlotUI> weaponIventorySlotUIs;
	[Space]
	public Text currentAmoInSelectedWeapon;
	public Text currentTotalAmoInSelectedWeapon;

	private Coroutine _updateRoutine;

	// INVENTORY STATE (aliniat cu weaponIventorySlotUIs)
	private class WeaponEntry { public WeaponData data; public int mag; public int reserve; }
	private List<WeaponEntry> _entries = new List<WeaponEntry>();
	private int _selectedIndex = -1;

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

		// inițializează lista de entries la dimensiunea sloturilor
		EnsureInventorySize();

		// dacă există deja o armă echipată la start, asigură-te că apare în inventory
		if (weaponController != null && weaponController.CurrentWeapon != null)
		{
			var w = weaponController.CurrentWeapon;
			if (FindIndexOf(w) == -1)
			{
				(int mag, int reserve, int _) = weaponController.GetAmmoInfo();
				AddWeaponInternal(w, mag, reserve, select: true, preferIndex: 0);
				Debug.Log($"[WeaponUI] Added starting weapon '{w.weaponName}' into slot 1.");
			}
			else
			{
				_selectedIndex = Mathf.Max(0, FindIndexOf(w));
			}
		}
	}

	private void OnDisable()
	{
		if (_updateRoutine != null) StopCoroutine(_updateRoutine);
	}

	private void Update()
	{
		// select via numeric keys 1..N
		if (weaponIventorySlotUIs != null && weaponIventorySlotUIs.Count > 0)
		{
			for (int i = 0; i < weaponIventorySlotUIs.Count && i < 9; i++)
			{
				if (Input.GetKeyDown(KeyCode.Alpha1 + i))
				{
					SelectSlotIndex(i);
				}
			}
			// 0 key maps to index 9 (10th slot)
			if (weaponIventorySlotUIs.Count >= 10 && Input.GetKeyDown(KeyCode.Alpha0))
			{
				SelectSlotIndex(9);
			}
		}
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

		// sync ammo of selected entry from controller
		if (_selectedIndex >= 0 && _selectedIndex < _entries.Count)
		{
			var sel = _entries[_selectedIndex];
			if (sel != null && sel.data != null && weaponController.CurrentWeapon == sel.data)
			{
				var info = weaponController.GetAmmoInfo();
				sel.mag = info.mag;
				sel.reserve = info.reserve;

				// current ammo texts
				if (currentAmoInSelectedWeapon != null) currentAmoInSelectedWeapon.text = sel.mag.ToString();
				if (currentTotalAmoInSelectedWeapon != null) currentTotalAmoInSelectedWeapon.text = (sel.mag + sel.reserve).ToString();
			}
		}

		// header texts
		var w = weaponController.CurrentWeapon;
		if (weaponNameText != null)
			weaponNameText.text = (w != null) ? w.weaponName : "No Weapon";

		if (ammoText != null)
		{
			var info = weaponController.GetAmmoInfo();
			ammoText.text = $"{info.mag} / {info.reserve}";
		}

		// refresh slot UIs
		RefreshSlotsUI();
	}

	private void RefreshSlotsUI()
	{
		EnsureInventorySize();
		for (int i = 0; i < weaponIventorySlotUIs.Count; i++)
		{
			var slotUI = weaponIventorySlotUIs[i];
			if (slotUI == null) continue;

			var entry = _entries[i];
			Sprite icon = entry?.data != null ? entry.data.weaponSpriteIcon : null;
			string name = entry?.data != null ? entry.data.weaponName : "";
			int total = entry != null ? (entry.mag + entry.reserve) : 0;
			bool selected = (i == _selectedIndex);

			slotUI.RefreshSlot(icon, i + 1, total, name, selected);
		}
	}

	private void EnsureInventorySize()
	{
		if (weaponIventorySlotUIs == null) return;
		int need = weaponIventorySlotUIs.Count;
		while (_entries.Count < need) _entries.Add(null);
		while (_entries.Count > need) _entries.RemoveAt(_entries.Count - 1);
		if (_selectedIndex >= _entries.Count) _selectedIndex = _entries.Count - 1;
	}

	private int FindFirstEmpty()
	{
		for (int i = 0; i < _entries.Count; i++)
			if (_entries[i] == null || _entries[i]?.data == null) return i;
		return -1;
	}

	private int FindIndexOf(WeaponData data)
	{
		if (data == null) return -1;
		for (int i = 0; i < _entries.Count; i++)
			if (_entries[i]?.data == data) return i;
		return -1;
	}

	private (int mag, int reserve) ComputeInitialAmmo(WeaponData d)
	{
		if (d == null) return (0, 0);
		int mag = Mathf.Min(d.magazineSize, d.startingReserveAmmo);
		int reserve = Mathf.Clamp(d.startingReserveAmmo - mag, 0, d.maxReserveAmmo);
		return (mag, reserve);
	}

	// Public API for pickups to add weapon to inventory
	public void AddWeaponToInventory(WeaponData data, bool select = true)
	{
		if (data == null) return;
		EnsureInventorySize();

		// already have it? just select
		int existing = FindIndexOf(data);
		if (existing != -1)
		{
			if (select) SelectSlotIndex(existing);
			Debug.Log($"[WeaponUI] Weapon '{data.weaponName}' already in slot {existing + 1}. Selected={select}");
			return;
		}

		var init = ComputeInitialAmmo(data);
		AddWeaponInternal(data, init.mag, init.reserve, select, preferIndex: FindFirstEmpty());
	}

	private void AddWeaponInternal(WeaponData data, int mag, int reserve, bool select, int preferIndex)
	{
		int idx = preferIndex;
		if (idx < 0) idx = (_selectedIndex >= 0 ? _selectedIndex : 0); // replace current if full

		_entries[idx] = new WeaponEntry { data = data, mag = mag, reserve = reserve };
		Debug.Log($"[WeaponUI] Put weapon '{data.weaponName}' into slot {idx + 1} (mag={mag}, reserve={reserve}).");

		if (select)
		{
			SelectSlotIndex(idx);
		}
		else
		{
			RefreshSlotsUI();
		}
	}

	private void SelectSlotIndex(int idx)
	{
		if (idx < 0 || idx >= _entries.Count) return;
		var entry = _entries[idx];
		if (entry == null || entry.data == null) return;

		// save current selected ammo back into its entry
		if (_selectedIndex >= 0 && _selectedIndex < _entries.Count && weaponController != null)
		{
			var curEntry = _entries[_selectedIndex];
			if (curEntry != null && curEntry.data != null && weaponController.CurrentWeapon == curEntry.data)
			{
				var info = weaponController.GetAmmoInfo();
				curEntry.mag = info.mag;
				curEntry.reserve = info.reserve;
			}
		}

		_selectedIndex = idx;

		// equip selected with stored ammo
		if (weaponController != null)
		{
			weaponController.EquipWeapon(entry.data, entry.mag, entry.reserve);
			Debug.Log($"[WeaponUI] Selected slot {idx + 1}: '{entry.data.weaponName}' (mag={entry.mag}, reserve={entry.reserve}).");
		}

		RefreshUI();
	}
}
