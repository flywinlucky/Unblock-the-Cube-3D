using UnityEngine;
using System.Collections;

public class Block : MonoBehaviour
{
    private MoveDirection _moveDirection;
    private LevelManager _levelManager;
    private bool _isMoving = false;
    private BoxCollider _collider;

    private void Awake()
    {
        _collider = GetComponent<BoxCollider>();
    }

    public void Initialize(MoveDirection dir, LevelManager manager)
    {
        _moveDirection = dir;
        _levelManager = manager;
    }

    private void OnMouseDown()
    {
        if (_isMoving) return;

        if (IsPathClear())
        {
            _isMoving = true;
            _collider.enabled = false; // Dezactivăm collider-ul pentru a nu bloca alte blocuri în mișcare
            _levelManager.OnBlockRemoved(this);
            StartCoroutine(MoveAndDestroy());
        }
    }

    private bool IsPathClear()
    {
        // Folosim un BoxCast, care este ca un Raycast, dar pentru o cutie.
        // Acesta va funcționa corect indiferent de mărimea și rotația blocului.
        Vector3 directionVector = GetDirectionVector();
        float maxDistance = 0.6f; // Puțin mai mult de jumătate de unitate

        // Ignorăm propriul collider folosind QueryTriggerInteraction.Ignore
        return !Physics.BoxCast(
            transform.position,
            _collider.size / 2.1f, // Mărimea cutiei de cast, puțin mai mică pentru a evita coliziuni false
            directionVector,
            transform.rotation,
            maxDistance,
            Physics.DefaultRaycastLayers,
            QueryTriggerInteraction.Ignore
        );
    }

    // Restul scriptului (MoveAndDestroy, GetDirectionVector) rămâne la fel
    private IEnumerator MoveAndDestroy()
    {
        Vector3 startPosition = transform.position;
        Vector3 endPosition = startPosition + GetDirectionVector() * 5.0f;
        float duration = 0.5f;
        float elapsed = 0f;
        while (elapsed < duration)
        {
            transform.position = Vector3.Lerp(startPosition, endPosition, elapsed / duration);
            elapsed += Time.deltaTime;
            yield return null;
        }
        Destroy(gameObject);
    }
    private Vector3 GetDirectionVector()
    {
        switch (_moveDirection)
        {
            case MoveDirection.Forward: return transform.forward;
            case MoveDirection.Back: return -transform.forward;
            case MoveDirection.Up: return transform.up;
            case MoveDirection.Down: return -transform.up;
            case MoveDirection.Left: return -transform.right;
            case MoveDirection.Right: return transform.right;
        }
        return Vector3.zero;
    }
}