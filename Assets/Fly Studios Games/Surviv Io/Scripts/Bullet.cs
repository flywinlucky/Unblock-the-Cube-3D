using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : MonoBehaviour
{
	public float speed = 15f;
	public float lifeTime = 5f;
	public float damage = 1f; // damage aplicat când lovește un DestroyableEntity

	private Rigidbody rb3;
	private Rigidbody2D rb2;
	private Vector3 _direction = Vector3.right; // default

	void Awake()
	{
		rb3 = GetComponent<Rigidbody>();
		rb2 = GetComponent<Rigidbody2D>();
	}

	void Start()
	{
		Destroy(gameObject, lifeTime);

		// Inițializare direcție din transform (fallback)
		_direction = transform.right;

		// Dacă avem Rigidbody, setăm viteza prin fizică
		if (rb3 != null)
		{
			rb3.velocity = _direction.normalized * speed;
			return;
		}

		if (rb2 != null)
		{
			rb2.velocity = (Vector2)_direction.normalized * speed;
			return;
		}

		// Dacă nu există Rigidbody, vom muta manual în Update
	}

	// Metodă publică pentru a seta direcția din WeaponControler imediat după Instantiate
	public void SetDirection(Vector3 dir)
	{
		if (dir == Vector3.zero) return;
		_direction = dir.normalized;

		// Ajustăm rotația vizuală: LookRotation pentru 3D, z-rotation pentru 2D
		if (rb3 != null)
		{
			transform.rotation = Quaternion.LookRotation(_direction);
			rb3.velocity = _direction * speed;
		}
		else if (rb2 != null)
		{
			float angle = Mathf.Atan2(_direction.y, _direction.x) * Mathf.Rad2Deg;
			transform.rotation = Quaternion.Euler(0f, 0f, angle);
			rb2.velocity = (Vector2)_direction * speed;
		}
		else
		{
			// fără rigidbody: doar setăm direcția; Update va muta projectile-ul
			float angle = Mathf.Atan2(_direction.y, _direction.x) * Mathf.Rad2Deg;
			transform.rotation = Quaternion.Euler(0f, 0f, angle);
		}
	}

	void Update()
	{
		// Fallback: dacă nu există Rigidbody, mutăm proiectilul manual
		if (rb3 == null && rb2 == null)
		{
			transform.position += _direction * speed * Time.deltaTime;
		}
	}

	// 3D trigger
	private void OnTriggerEnter(Collider other)
	{
		if (other == null) return;

		var dest = other.GetComponent<DestroyableEntity>();
		if (dest != null)
		{
			dest.OnHitByBullet(this);
		}

		Destroy(gameObject);
	}

	// 2D trigger
	private void OnTriggerEnter2D(Collider2D other)
	{
		if (other == null) return;

		var dest = other.GetComponent<DestroyableEntity>();
		if (dest != null)
		{
			dest.OnHitByBullet(this);
		}

		Destroy(gameObject);
	}
}
