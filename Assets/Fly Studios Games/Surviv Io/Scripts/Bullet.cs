using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : MonoBehaviour
{
	private Rigidbody2D rb2;
	private Vector3 _direction = Vector3.right; // default
	private Vector3 _startPos;

	// internal state set from WeaponData via Init()
	private float _speed;
	private float _damage;
	private float _maxDistance;

	void Awake()
	{
		rb2 = GetComponent<Rigidbody2D>();
	}

	void Start()
	{
		_startPos = transform.position;

		// Inițializare direcție din transform (fallback)
		_direction = transform.right;

		// Dacă avem Rigidbody2D, setăm viteza prin fizică 2D
		if (rb2 != null)
		{
			rb2.velocity = (Vector2)_direction.normalized * _speed;
			return;
		}

		// Dacă nu există Rigidbody2D, vom muta manual în Update
	}

	// Inițializează parametrii din WeaponData
	public void Init(float damage, float speed, float range)
	{
		_damage = damage;
		_speed = speed;
		_maxDistance = range;
	}

	// Metodă publică pentru a seta direcția din WeaponControler imediat după Instantiate
	public void SetDirection(Vector3 dir)
	{
		if (dir == Vector3.zero) return;
		_direction = dir.normalized;

		// Ajustăm rotația vizuală: z-rotation pentru 2D și aplicăm viteză
		float angle = Mathf.Atan2(_direction.y, _direction.x) * Mathf.Rad2Deg;
		transform.rotation = Quaternion.Euler(0f, 0f, angle);

		if (rb2 != null)
		{
			rb2.velocity = (Vector2)_direction * _speed;
		}
	}

	void Update()
	{
		// Fallback: dacă nu există Rigidbody2D, mutăm proiectilul manual
		if (rb2 == null)
		{
			transform.position += _direction * _speed * Time.deltaTime;
		}

		// distrugem după ce depășește distanța (range)
		if (_maxDistance > 0f && Vector3.Distance(_startPos, transform.position) >= _maxDistance)
		{
			Destroy(gameObject);
		}
	}

	// Coliziune 2D (nu trigger)
	private void OnCollisionEnter2D(Collision2D collision)
	{
		if (collision == null) return;

		var dest = collision.collider.GetComponent<DestroyableEntity>();
		if (dest != null)
		{
			// pasăm damage-ul curent
			dest.OnHitByBullet(this);
		}

		Destroy(gameObject);
	}

	// Helper pentru accesarea damage-ului curent de către ținte
	public float GetDamage()
	{
		return _damage;
	}
}
