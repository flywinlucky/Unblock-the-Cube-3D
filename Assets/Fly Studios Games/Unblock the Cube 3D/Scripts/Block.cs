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
            _collider.enabled = false;
            _levelManager.OnBlockRemoved(this);
            StartCoroutine(MoveAndDestroy());
        }
    }

    private bool IsPathClear()
    {
        Vector3 directionVector = GetDirectionVector();
        float maxDistance = 10f;

        return !Physics.Raycast(transform.position, directionVector, maxDistance);
    }

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

    // --- MODIFICAT: Folosim vectori locali (relativi la rotația obiectului) ---
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

    private void OnDrawGizmos()
    {
        // Gizmo-ul va folosi acum aceeași funcție modificată,
        // deci se va roti și el odată cu obiectul.
        if (_collider == null) { _collider = GetComponent<BoxCollider>(); }

        bool isClear = IsPathClear();
        Gizmos.color = isClear ? Color.green : Color.red;

        Vector3 direction = GetDirectionVector();
        float distance = 10f;

        Gizmos.DrawRay(transform.position, direction * distance);
    }
}