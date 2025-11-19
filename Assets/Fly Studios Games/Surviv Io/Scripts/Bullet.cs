using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : MonoBehaviour
{
	public float speed = 15f;
	public float lifeTime = 5f;
	public float damage = 1f; // damage aplicat când lovește un DestroyableEntity

	private Rigidbody2D rb2;
	private Vector3 _direction = Vector3.right; // default

	void Awake()
	{
		rb2 = GetComponent<Rigidbody2D>();
	}

	void Start()
	{
		Destroy(gameObject, lifeTime);

		// Inițializare direcție din transform (fallback)
		_direction = transform.right;

		// Dacă avem Rigidbody2D, setăm viteza prin fizică 2D
		if (rb2 != null)
		{
			rb2.velocity = (Vector2)_direction.normalized * speed;
			return;
		}

		// Dacă nu există Rigidbody2D, vom muta manual în Update
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
			rb2.velocity = (Vector2)_direction * speed;
		}
	}

	void Update()
	{
		// Fallback: dacă nu există Rigidbody2D, mutăm proiectilul manual
		if (rb2 == null)
		{
			transform.position += _direction * speed * Time.deltaTime;
		}
	}

	// Coliziune 2D (nu trigger)
	private void OnCollisionEnter2D(Collision2D collision)
	{
		if (collision == null) return;

		var dest = collision.collider.GetComponent<DestroyableEntity>();
		if (dest != null)
		{
			dest.OnHitByBullet(this);
		}

		Destroy(gameObject);
	}
}
