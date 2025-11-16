using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponControler : MonoBehaviour
{
    [Header("Weapon")]
    public GameObject bulletPrefab;
    public Transform muzzle; // where bullets are spawned
    public float fireRate = 0.18f; // secunde între focuri

    private float _nextFireTime = 0f;

    // Poți apela din Player: weaponController.Fire();
    public void Fire()
    {
        if (bulletPrefab == null || muzzle == null) return;
        if (Time.time < _nextFireTime) return;

        _nextFireTime = Time.time + fireRate;
        GameObject inst = Instantiate(bulletPrefab, muzzle.position, muzzle.rotation);

        // Dacă bullet are scriptul Bullet, îi transmitem direcția explicit pentru siguranță.
        var bulletComp = inst.GetComponent<Bullet>();
        if (bulletComp != null)
        {
            // Proiectil pentru 2D: folosește întotdeauna axa +X a muzzle (muzzle.right)
            Vector3 dir = muzzle.right.normalized;

            // Setăm rotația instanțiatului pe axa Z în funcție de direcție
            float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
            inst.transform.rotation = Quaternion.Euler(0f, 0f, angle);

            bulletComp.SetDirection(dir);
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
