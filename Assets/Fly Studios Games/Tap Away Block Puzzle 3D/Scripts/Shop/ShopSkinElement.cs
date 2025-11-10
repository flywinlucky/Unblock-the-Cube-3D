using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Tap_Away_Block_Puzzle_3D
{
    /// <summary>
    /// UI element for a single shop skin entry.
    /// Responsible for updating visuals and forwarding clicks to ShopManager.
    /// </summary>
    public class ShopSkinElement : MonoBehaviour
    {
        #region Inspector - UI References

        [Header("UI References")]
        [Tooltip("Text that displays the skin name.")]
        public Text skinName;

        [Tooltip("Text that displays the skin price or status.")]
        public Text skinPrice;

        [Tooltip("Icon image for the skin.")]
        public Image skinSprite;

        [Tooltip("Primary action button (Buy / Select).")]
        public Button actionButton;

        [Tooltip("Optional locked overlay panel shown when skin is not owned.")]
        public GameObject lockedPanel;

        [Tooltip("Icon or object indicating coin cost; hidden when owned.")]
        public GameObject coinIconImage;

        [Tooltip("Sprite used for button when this skin is selected.")]
        public Sprite selectedSkinButtonSprite;

        [Tooltip("Sprite used for button when this skin is not selected.")]
        public Sprite deselectedSkinButtonSprite;

        #endregion

        #region Internal State

        private string _skinId;
        private int _price;
        private bool _owned;
        private bool _selected;
        private ShopManager _manager;

        #endregion

        /// <summary>
        /// Initialize the UI element.
        /// </summary>
        public void Initialize(string id, string name, int price, Sprite icon, bool owned, bool selected, ShopManager manager)
        {
            _skinId = id;
            _manager = manager;
            _price = price;
            _owned = owned;
            _selected = selected;

            if (skinName != null) skinName.text = name;
            if (skinSprite != null) skinSprite.sprite = icon;

            UpdateUI();

            if (actionButton != null)
            {
                actionButton.onClick.RemoveAllListeners();
                actionButton.onClick.AddListener(OnClicked);
            }

            UpdateLocked();
        }

        private void UpdateUI()
        {
            if (skinPrice != null)
            {
                if (_owned)
                    skinPrice.text = _selected ? "Selected" : "Owned";
                else
                    skinPrice.text = _price.ToString();
            }

            if (actionButton != null)
            {
                actionButton.interactable = !_selected;

                Image btnImg = actionButton.image;
                if (btnImg != null)
                {
                    if (_selected && selectedSkinButtonSprite != null)
                        btnImg.sprite = selectedSkinButtonSprite;
                    else if (deselectedSkinButtonSprite != null)
                        btnImg.sprite = deselectedSkinButtonSprite;
                }
            }

            UpdateLocked();
        }

        private void UpdateLocked()
        {
            if (lockedPanel != null) lockedPanel.SetActive(!_owned);
            if (coinIconImage != null) coinIconImage.SetActive(!_owned);
        }

        private void OnClicked()
        {
            if (_manager != null)
            {
                _manager.OnElementClicked(_skinId, _price, _owned);
            }
        }
    }
}