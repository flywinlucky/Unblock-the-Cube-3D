using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class WeaponIventorySlotUI : MonoBehaviour
{
    public Text weapon_Slot_Index;
    public Image weapon_Slot_Icon;
    public Text weapon_Slot_AllAmoCount;
    public Text weapon_Slot_Name;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    // Refresh contents; selected can be used to add a small visual cue (optional)
    public void RefreshSlot(Sprite icon, int indexOneBased, int totalAmmo, string weaponName, bool selected)
    {
        bool hasWeapon = icon != null && !string.IsNullOrEmpty(weaponName);

        // Index always visible & numbered
        if (weapon_Slot_Index != null)
        {
            weapon_Slot_Index.gameObject.SetActive(true);
            weapon_Slot_Index.text = indexOneBased.ToString();
        }

        // Toggle others based on weapon presence
        if (weapon_Slot_Icon != null)
        {
            weapon_Slot_Icon.gameObject.SetActive(hasWeapon);
            weapon_Slot_Icon.sprite = icon;
            var c = weapon_Slot_Icon.color;
            c.a = (selected && hasWeapon) ? 1f : 0.6f;
            weapon_Slot_Icon.color = c;
        }

        if (weapon_Slot_AllAmoCount != null)
        {
            weapon_Slot_AllAmoCount.gameObject.SetActive(hasWeapon);
            weapon_Slot_AllAmoCount.text = hasWeapon ? totalAmmo.ToString() : "";
        }

        if (weapon_Slot_Name != null)
        {
            weapon_Slot_Name.gameObject.SetActive(hasWeapon);
            weapon_Slot_Name.text = hasWeapon ? weaponName : "";
        }
    }
}
