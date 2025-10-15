using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ShopManager : MonoBehaviour
{
	[System.Serializable]
	public class ShopSkinData
	{
		// id removed - generated automatically by index
		public string displayName;
		public int price;
		public Sprite icon;
		public Material material;
	}

	[Header("Shop Setup")]
	public GameObject shopSkinElementPrefab; // prefab ShopSkinElement
	public Transform contentRoot; // unde vom instanția elementele UI
	public List<ShopSkinData> skins = new List<ShopSkinData>();

	[Header("References")]
	public LevelManager levelManager; // pentru a aplica skin la blocuri

	private const string SelectedSkinKey = "SelectedSkin";
	private HashSet<string> _owned = new HashSet<string>();
	public string selectedSkinId;
	public Material selectedMaterial;

	private void Start()
	{
		LoadOwned();
		LoadSelected();
		PopulateShop();
	}

	private string GetSkinId(int index) => $"skin_{index}";

	private int GetIndexFromId(string id)
	{
		if (string.IsNullOrEmpty(id)) return -1;
		if (!id.StartsWith("skin_")) return -1;
		if (int.TryParse(id.Substring(5), out int idx)) return idx;
		return -1;
	}

	private void LoadOwned()
	{
		_owned.Clear();
		for (int i = 0; i < skins.Count; i++)
		{
			string id = GetSkinId(i);
			if (PlayerPrefs.GetInt("SkinOwned_" + id, 0) == 1) _owned.Add(id);
		}
	}

	private void LoadSelected()
	{
		selectedSkinId = PlayerPrefs.GetString(SelectedSkinKey, "");
		selectedMaterial = null;
		int idx = GetIndexFromId(selectedSkinId);
		if (idx >= 0 && idx < skins.Count) selectedMaterial = skins[idx].material;
	}

	private void PopulateShop()
	{
		if (shopSkinElementPrefab == null || contentRoot == null) return;
		// curățăm conținutul
		for (int i = contentRoot.childCount - 1; i >= 0; i--) DestroyImmediate(contentRoot.GetChild(i).gameObject);

		for (int i = 0; i < skins.Count; i++)
		{
			var s = skins[i];
			string id = GetSkinId(i);
			GameObject go = Instantiate(shopSkinElementPrefab, contentRoot);
			ShopSkinElement el = go.GetComponent<ShopSkinElement>();
			if (el != null)
			{
				bool owned = _owned.Contains(id);
				bool isSelected = id == selectedSkinId;
				el.Initialize(id, s.displayName, s.price, s.icon, owned, isSelected, this);
			}
		}
	}

	// apelat din element UI când jucătorul apasă butonul
	public void OnElementClicked(string skinId, int price, bool owned)
	{
		if (owned)
		{
			SelectSkin(skinId);
			return;
		}

		// încercăm cumpărarea
		if (levelManager == null)
		{
			Debug.LogWarning("ShopManager needs LevelManager reference for coin logic.");
			return;
		}

		if (levelManager.SpendCoins(price))
		{
			BuySkin(skinId);
		}
		else
		{
			// notificare via LevelManager.NotificationManager dacă e legat
			if (levelManager.notificationManager != null)
				levelManager.notificationManager.ShowNotification("Not enough coins", 2f);
		}
	}

	private void BuySkin(string skinId)
	{
		_owned.Add(skinId);
		PlayerPrefs.SetInt("SkinOwned_" + skinId, 1);
		PlayerPrefs.Save();
		PopulateShop();
		// imediat selectăm cumpăratul
		SelectSkin(skinId);
	}

	public void SelectSkin(string skinId)
	{
		int idx = GetIndexFromId(skinId);
		if (idx < 0 || idx >= skins.Count) return;

		selectedSkinId = skinId;
		selectedMaterial = skins[idx].material;
		PlayerPrefs.SetString(SelectedSkinKey, selectedSkinId);
		PlayerPrefs.Save();

		PopulateShop();

		// aplicăm skin la toate blocurile din scena curentă (LevelManager)
		if (levelManager != null)
		{
			foreach (var b in levelManager.GetActiveBlocks())
			{
				if (b != null) b.ApplySkin(selectedMaterial);
			}
		}
	}
}
