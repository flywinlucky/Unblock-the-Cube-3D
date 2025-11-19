using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class WeaponIventorySlotUI : MonoBehaviour
{
    public Image weapon_Slot_Icon;
    public Text weapon_Slot_Index;
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
        if (weapon_Slot_Icon != null) weapon_Slot_Icon.sprite = icon;
        if (weapon_Slot_Index != null) weapon_Slot_Index.text = indexOneBased.ToString();
        if (weapon_Slot_AllAmoCount != null) weapon_Slot_AllAmoCount.text = totalAmmo.ToString();
        if (weapon_Slot_Name != null) weapon_Slot_Name.text = string.IsNullOrEmpty(weaponName) ? "-" : weaponName;

        // optional tiny feedback for selected
        if (weapon_Slot_Icon != null)
        {
            var c = weapon_Slot_Icon.color;
            c.a = selected ? 1f : 0.6f;
            weapon_Slot_Icon.color = c;
        }
    }
}
