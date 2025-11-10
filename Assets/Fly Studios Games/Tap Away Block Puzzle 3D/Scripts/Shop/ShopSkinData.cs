using UnityEngine;

namespace Tap_Away_Block_Puzzle_3D
{
    [CreateAssetMenu(fileName = "NewShopSkin", menuName = "Tap Away Block Puzzle 3D/New Skin")]
    public class ShopSkinData : ScriptableObject
    {
        [Header("Identification")]
        [Tooltip("Unique identifier for this skin (used for persistence).")]
        public string id;

        [Tooltip("Display name shown in the shop UI.")]
        public string displayName;

        [Header("Pricing & Visuals")]
        [Tooltip("Price in coins for this skin.")]
        public int price;

        [Tooltip("Icon used in the shop UI.")]
        public Sprite icon;

        [Tooltip("Material applied to blocks when this skin is selected.")]
        public Material material;

        [Tooltip("Arrow tint color applied to blocks when this skin is selected.")]
        public Color arrowColor;
    }
}