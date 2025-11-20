using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponSpawnBulletPoint : MonoBehaviour
{
    public Transform weaponBulletSpawn;
    public WeaponControler weaponControler;
    void Start()
    {
        weaponControler = FindObjectOfType<WeaponControler>();
        weaponControler.muzzle = weaponBulletSpawn;
    }
}