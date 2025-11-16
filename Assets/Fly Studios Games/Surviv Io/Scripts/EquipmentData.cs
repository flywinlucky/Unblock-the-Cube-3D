using UnityEngine;

[CreateAssetMenu(fileName = "NewEquipmentData", menuName = "SurvivIo/Equipment Data")]
public class EquipmentData : ScriptableObject
{
    [Header("Equipment Properties")]
    public Sprite equipmentSpriteIcon; // Icon for the equipment
    public int equipmentLevel;         // Level of the equipment
    public float damageReduction;        // Damage reduction provided by the equipment
}
