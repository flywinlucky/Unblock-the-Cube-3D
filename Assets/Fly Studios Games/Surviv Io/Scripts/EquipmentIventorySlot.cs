using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EquipmentIventorySlot : MonoBehaviour
{
    public Image equipment_Icon_Image;
    public Text equipment_level_Text;
    public GameObject equipament_Gameobject;

    public void RefreshEquipamentSlot(Sprite equipament_icon, int equipament_Level)
    {
        equipment_Icon_Image.sprite = equipament_icon;
        equipment_level_Text.text = "LVL." + equipament_Level.ToString();
    }
}