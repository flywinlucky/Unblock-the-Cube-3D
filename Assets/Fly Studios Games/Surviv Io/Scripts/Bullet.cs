using UnityEngine;

public class Bullet : MonoBehaviour
{
    private Rigidbody2D rb2;
    private Vector3 _direction;
    private Vector3 _startPos;

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

        // Dacă nu i s-a dat direcție din WeaponControler, folosim transform.right
        if (_direction == Vector3.zero)
            _direction = transform.right;

        // Aplică viteza în Rigidbody (mișcare fizică, ultra smooth)
        if (rb2 != null)
            rb2.velocity = (Vector2)_direction * _speed;
    }

    // Este apelată imediat după Instantiate, înainte de Start()
    public void Init(float damage, float speed, float range)
    {
        _damage = damage;
        _speed = speed;
        _maxDistance = range;
    }

    // WeaponControler apelează asta înainte de Start()
    public void SetDirection(Vector3 dir)
    {
        if (dir == Vector3.zero) return;

        _direction = dir.normalized;

        // Rotim glonțul după direcție
        float angle = Mathf.Atan2(_direction.y, _direction.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0f, 0f, angle);

        if (rb2 != null)
            rb2.velocity = (Vector2)_direction * _speed;
    }

    void Update()
    {
        // DOAR verificare distanță. NU mișcare aici.
        if (_maxDistance > 0f &&
            Vector3.Distance(_startPos, transform.position) >= _maxDistance)
        {
            Destroy(gameObject);
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision == null) return;

        var dest = collision.collider.GetComponent<DestroyableEntity>();
        if (dest != null)
            dest.OnHitByBullet(this);

        Destroy(gameObject);
    }

    public float GetDamage()
    {
        return _damage;
    }
}
