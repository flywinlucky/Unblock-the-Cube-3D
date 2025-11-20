using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class DestroyableEntity : MonoBehaviour
{
    [Header("Health")]
    private float maxHealth = 100f;
    private float _currentHealth;

    [Header("Hit Effect")]
    public float shakeStrength = 0.1f;   // cât de tare se mișcă
    public float shakeDuration = 0.15f;  // durata efectului
    public float returnEase = 0.3f;      // cât de smooth revine

    private Tween _currentShake;
    private Vector3 _originalLocalPos;

    void Start()
    {
        _currentHealth = maxHealth;
        _originalLocalPos = transform.localPosition;
    }

    public virtual void OnHitByBullet(Bullet bullet)
    {
        if (bullet == null) return;
        TakeDamage(bullet.GetDamage());
    }

    public void TakeDamage(float amount)
    {
        if (amount <= 0f) return;

        _currentHealth -= amount;
        _currentHealth = Mathf.Max(0f, _currentHealth);

        PlayHitShake();

        if (_currentHealth <= 0f)
        {
            Destroy(gameObject);
        }
    }

    private void PlayHitShake()
    {
        // anulăm shake-ul curent dacă există
        if (_currentShake != null && _currentShake.IsActive())
            _currentShake.Kill(true);

        // resetăm poziția ca să nu acumuleze offset
        transform.localPosition = _originalLocalPos;

        // direcție random 2D
        Vector2 offset = Random.insideUnitCircle * shakeStrength;

        // animăm spre offset și înapoi
        _currentShake = transform.DOLocalMove(
            _originalLocalPos + (Vector3)offset,
            shakeDuration * 0.5f
        )
        .SetLoops(2, LoopType.Yoyo)
        .SetEase(Ease.OutQuad)
        .OnComplete(() => transform.localPosition = _originalLocalPos);
    }
}
