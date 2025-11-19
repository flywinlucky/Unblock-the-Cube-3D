using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponControler : MonoBehaviour
{
	[Header("Weapon (Modular)")]
	public WeaponData startingWeapon; // setezi ScriptableObject în inspector
	public Transform muzzle; // where bullets are spawned

	// state
	private WeaponData _weapon;
	private int _currentMagazine = 0;
	private int _reserveAmmo = 0;
	private float _nextFireTime = 0f;
	private bool _isReloading = false;

	// public acces pentru UI sau alte sisteme
	public WeaponData CurrentWeapon => _weapon;
	public int CurrentMagazine => _currentMagazine;
	public int ReserveAmmo => _reserveAmmo;
	public bool IsReloading => _isReloading;

	// Reload events + remaining time
	public event Action<float> OnReloadStarted;   // duration (seconds)
	public event Action<float> OnReloadProgress;  // remaining (seconds)
	public event Action OnReloadFinished;
	public float ReloadRemainingTime { get; private set; } = 0f;

	private void Start()
	{
		if (startingWeapon != null)
			EquipWeapon(startingWeapon);
	}

	// Echipăm o armă (încărcăm magazinul și rezervă)
	public void EquipWeapon(WeaponData data)
	{
		_weapon = data;
		if (_weapon == null) return;

		// initialize magazine and reserve: umple magazinul și pune restul în rezervă
		_currentMagazine = Mathf.Min(_weapon.magazineSize, _weapon.startingReserveAmmo);
		_reserveAmmo = Mathf.Clamp(_weapon.startingReserveAmmo - _currentMagazine, 0, _weapon.maxReserveAmmo);
		_nextFireTime = 0f;
		_isReloading = false;
	}

	// OVERLOAD: echipăm arma cu muniție explicită (folosită de UI inventory la switch)
	public void EquipWeapon(WeaponData data, int currentMag, int reserve)
	{
		_weapon = data;
		if (_weapon == null) return;

		_currentMagazine = Mathf.Clamp(currentMag, 0, _weapon.magazineSize);
		_reserveAmmo = Mathf.Clamp(reserve, 0, _weapon.maxReserveAmmo);
		_nextFireTime = 0f;
		_isReloading = false;
	}

	// Fire folosește datele din WeaponData; dacă ammo 0 nu trage
	public void Fire()
	{
		if (_weapon == null || muzzle == null) return;
		if (Time.time < _nextFireTime) return;
		if (_isReloading) return;

		// dacă nu avem gloanțe în magazin, încercăm reload automat (dacă există rezervă)
		if (_currentMagazine <= 0)
		{
			Reload();
			return;
		}

		_nextFireTime = Time.time + _weapon.fireRate;

		GameObject inst = Instantiate(_weapon.bulletPrefab, muzzle.position, muzzle.rotation);
		var bulletComp = inst.GetComponent<Bullet>();
		if (bulletComp != null)
		{
			// direcția pentru 2D: muzzle.right este "în față"
			Vector3 dir = muzzle.right.normalized;
			float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
			inst.transform.rotation = Quaternion.Euler(0f, 0f, angle);

			// setăm parametrii proiectilului din WeaponData
			bulletComp.damage = _weapon.damage;
			bulletComp.speed = _weapon.bulletSpeed;
			bulletComp.SetDirection(dir);
		}

		_currentMagazine = Mathf.Max(0, _currentMagazine - 1);

		// auto-reload dacă magazinul a ajuns la 0 după împușcare și există rezervă
		if (_currentMagazine <= 0 && _reserveAmmo > 0)
		{
			Reload();
		}
	}

	// Reload: transferă din rezervă în magazin după un delay (weapon.reloadTime)
	public void Reload()
	{
		if (_weapon == null) return;
		if (_isReloading) return;
		if (_currentMagazine >= _weapon.magazineSize) return;
		if (_reserveAmmo <= 0) return;

		int needed = _weapon.magazineSize - _currentMagazine;
		int toLoad = Mathf.Min(needed, _reserveAmmo);

		// dacă nu există timp de reload -> instant
		if (_weapon.reloadTime <= 0f)
		{
			_currentMagazine += toLoad;
			_reserveAmmo -= toLoad;
			_isReloading = false;
			OnReloadStarted?.Invoke(0f);
			OnReloadProgress?.Invoke(0f);
			OnReloadFinished?.Invoke();
			return;
		}

		_isReloading = true;
		StartCoroutine(ReloadRoutine(toLoad, _weapon.reloadTime));
	}

	private IEnumerator ReloadRoutine(int toLoad, float duration)
	{
		ReloadRemainingTime = duration;
		OnReloadStarted?.Invoke(duration);

		while (ReloadRemainingTime > 0f)
		{
			OnReloadProgress?.Invoke(ReloadRemainingTime);
			yield return null;
			ReloadRemainingTime -= Time.deltaTime;
		}

		// finalizează: aplică transferul de ammo abia acum
		_currentMagazine += toLoad;
		_reserveAmmo -= toLoad;

		_isReloading = false;
		ReloadRemainingTime = 0f;
		OnReloadProgress?.Invoke(0f);
		OnReloadFinished?.Invoke();
	}

	// Adaugă muniție în rezervă (folosit de AmmoPickup). Returnează cât a fost adăugat
	public int AddAmmo(int amount)
	{
		if (_weapon == null || amount <= 0) return 0;
		int before = _reserveAmmo;
		_reserveAmmo = Mathf.Clamp(_reserveAmmo + amount, 0, _weapon.maxReserveAmmo);
		return _reserveAmmo - before;
	}

	// API util pentru UI / debug
	public (int mag, int reserve, int magSize) GetAmmoInfo()
	{
		return (_currentMagazine, _reserveAmmo, _weapon != null ? _weapon.magazineSize : 0);
	}
}
