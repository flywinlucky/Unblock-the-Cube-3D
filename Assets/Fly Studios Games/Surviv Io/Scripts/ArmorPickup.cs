using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ArmorPickup : MonoBehaviour
{
    public EquipmentData equipmentData; // ScriptableObject containing armor data
    public SpriteRenderer armor_icon;

    private void Start()
    {
        // Set the armor_icon sprite from EquipmentData
        if (armor_icon != null && equipmentData != null)
        {
            armor_icon.sprite = equipmentData.equipmentSpriteIcon;
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        TryApplyPickup(other.gameObject);
    }

    private void OnTriggerEnter(Collider other)
    {
        TryApplyPickup(other.gameObject);
    }

    private void TryApplyPickup(GameObject other)
    {
        if (other == null || equipmentData == null) return;

        var playerUI = other.GetComponentInChildren<PlayerUI>();
        var playerHealth = other.GetComponentInChildren<PlayerHealth>();
        if (playerUI == null || playerHealth == null) return;

        // Apply equipment data to the appropriate slot
        if (equipmentData.equipmentType == EquipmentData.EquipmentType.Helmets)
        {
            playerUI.helment_Slot.RefreshEquipamentSlot(equipmentData.equipmentSpriteIcon, equipmentData.equipmentLevel);
        }
        else if (equipmentData.equipmentType == EquipmentData.EquipmentType.Vests)
        {
            playerUI.vest_Slot.RefreshEquipamentSlot(equipmentData.equipmentSpriteIcon, equipmentData.equipmentLevel);
        }

        // Apply damage reduction to the player
        playerHealth.ApplyDamageReduction(equipmentData.damageReduction);

        // Destroy the pickup after applying
        Destroy(gameObject);
    }
}