using System.Collections.Generic;
using UnityEngine;

namespace Tap_Away_Block_Puzzle_3D
{
    /// <summary>
    /// Manages the in-game skin shop: populates UI elements, handles purchases and selection,
    /// and applies the selected skin to active blocks.
    /// Functionality is preserved; this file is refactored for readability and Asset Store standards.
    /// </summary>
    public class ShopManager : MonoBehaviour
    {
        #region Inspector Fields

        [Header("Shop Setup")]
        [Tooltip("Prefab used to create each shop UI element.")]
        public GameObject shopSkinElementPrefab;

        [Tooltip("Parent transform where shop elements will be instantiated.")]
        public Transform contentRoot;

        [Tooltip("List of available skins (ScriptableObjects).")]
        public List<ShopSkinData> skins = new List<ShopSkinData>();

        [Header("References")]
        [Tooltip("Reference to LevelManager to apply skins to blocks and perform coin transactions.")]
        public LevelManager levelManager;

        #endregion

        #region Persistence & State

        private const string SelectedSkinKey = "SelectedSkin";
        private HashSet<string> _owned = new HashSet<string>();
        [HideInInspector] public ShopSkinData selectedSkin;

        #endregion

        #region Unity Events

        private void Start()
        {
            LoadOwned();
            LoadSelected();
            PopulateShop();
        }

        #endregion

        #region Shop Logic

        private void LoadOwned()
        {
            _owned.Clear();
            foreach (var skin in skins)
            {
                if (PlayerPrefs.GetInt("SkinOwned_" + skin.id, 0) == 1)
                {
                    _owned.Add(skin.id);
                }
            }
        }

        private void LoadSelected()
        {
            string selectedSkinId = PlayerPrefs.GetString(SelectedSkinKey, "");
            selectedSkin = skins.Find(skin => skin.id == selectedSkinId);
        }

        /// <summary>
        /// Populate the shop UI by instantiating ShopSkinElement prefabs.
        /// </summary>
        private void PopulateShop()
        {
            if (shopSkinElementPrefab == null || contentRoot == null) return;

            // Clear previous content
            for (int i = contentRoot.childCount - 1; i >= 0; i--)
            {
                DestroyImmediate(contentRoot.GetChild(i).gameObject);
            }

            foreach (var skin in skins)
            {
                GameObject go = Instantiate(shopSkinElementPrefab, contentRoot);
                ShopSkinElement el = go.GetComponent<ShopSkinElement>();
                if (el != null)
                {
                    bool owned = _owned.Contains(skin.id);
                    bool isSelected = selectedSkin == skin;
                    el.Initialize(skin.id, skin.displayName, skin.price, skin.icon, owned, isSelected, this);
                }
            }
        }

        /// <summary>
        /// Called by UI element when a skin item is clicked.
        /// If the skin is already owned it will be selected, otherwise attempt purchase.
        /// </summary>
        public void OnElementClicked(string skinId, int price, bool owned)
        {
            ShopSkinData skin = skins.Find(s => s.id == skinId);
            if (skin == null) return;

            if (owned)
            {
                SelectSkin(skin);
                return;
            }

            if (levelManager == null)
            {
                Debug.LogWarning("ShopManager needs LevelManager reference for coin logic.");
                return;
            }

            if (levelManager.SpendCoins(price))
            {
                BuySkin(skin);
            }
            else
            {
                if (levelManager.notificationManager != null)
                    levelManager.notificationManager.ShowNotification("Not enough coins", 2f);
            }
        }

        private void BuySkin(ShopSkinData skin)
        {
            _owned.Add(skin.id);
            PlayerPrefs.SetInt("SkinOwned_" + skin.id, 1);
            PlayerPrefs.Save();
            PopulateShop();

            // Immediately select the purchased skin
            SelectSkin(skin);
        }

        /// <summary>
        /// Selects the given skin and applies it to all active blocks.
        /// Persists selection.
        /// </summary>
        public void SelectSkin(ShopSkinData skin)
        {
            selectedSkin = skin;
            PlayerPrefs.SetString(SelectedSkinKey, skin.id);
            PlayerPrefs.Save();
            PopulateShop();

            if (levelManager != null)
            {
                foreach (var block in levelManager.GetActiveBlocks())
                {
                    if (block != null)
                    {
                        block.ApplySkin(skin.material);
                        block.arrowCollor = skin.arrowColor;
                        block.ApplyArrowColor();
                    }
                }
            }
        }

        /// <summary>
        /// Returns the Material for the currently selected skin or null.
        /// </summary>
        public Material selectedMaterial
        {
            get { return selectedSkin != null ? selectedSkin.material : null; }
        }

        #endregion
    }
}