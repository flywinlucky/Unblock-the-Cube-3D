using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Tap_Away_Block_Puzzle_3D
{
    /// <summary>
    /// Handles the open/close behavior for the buy power-up panel and updates the button visuals.
    /// </summary>
    public class SuperPowerUI : MonoBehaviour
    {
        [Header("UI References")]
        [Tooltip("Button that toggles the buy power panel.")]
        public Button openBuyPowerPanel_button;

        [Tooltip("Image component of the toggle button (icon).")]
        public Image openBuyPowerPanel_button_image;

        [Tooltip("Icon shown when the panel is closed.")]
        public Sprite close_icon;

        [Tooltip("Icon shown when the panel is open.")]
        public Sprite plus_icon;

        [Tooltip("Button color when the panel is open.")]
        public Color open_color;

        [Tooltip("Button color when the panel is closed.")]
        public Color close_color;

        [Tooltip("The panel that contains power-up purchase UI.")]
        public GameObject BuyPowerPanel;

        private bool isPanelOpen = false;

        private void Start()
        {
            if (openBuyPowerPanel_button != null)
                openBuyPowerPanel_button.onClick.AddListener(ToggleBuyPowerPanel);

            if (BuyPowerPanel != null)
                BuyPowerPanel.SetActive(isPanelOpen);
        }

        /// <summary>
        /// Toggle open/close state of the buy-power panel and update icon and button color.
        /// </summary>
        public void ToggleBuyPowerPanel()
        {
            isPanelOpen = !isPanelOpen;

            if (BuyPowerPanel != null) BuyPowerPanel.SetActive(isPanelOpen);

            if (openBuyPowerPanel_button_image != null)
                openBuyPowerPanel_button_image.sprite = isPanelOpen ? plus_icon : close_icon;

            if (openBuyPowerPanel_button != null)
                openBuyPowerPanel_button.image.color = isPanelOpen ? close_color : open_color;
        }
    }
}