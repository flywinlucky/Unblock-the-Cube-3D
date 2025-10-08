using UnityEngine;
using System.Collections;

public class Block : MonoBehaviour
{
    [Tooltip("Trage aici obiectul copil 'ArrowShell' din Prefab.")]
    public Renderer arrowShellRenderer;

    [Tooltip("Punctul de referință pentru mișcare și Raycast.")]
    public Transform raycastPoint;

    private MoveDirection _moveDirection;
    private LevelManager _levelManager;
    private bool _isMoving = false;
    private BoxCollider _collider;

    private static readonly int MoveDirectionID = Shader.PropertyToID("_MoveDirection");

    private void Awake()
    {
        _collider = GetComponent<BoxCollider>();

        if (arrowShellRenderer == null)
            Debug.LogError("ArrowShell Renderer nu este asignat în Inspector!", this);

        if (raycastPoint == null)
            Debug.LogWarning("Raycast Point nu este asignat! Folosim transform ca fallback.", this);
    }

    public void Initialize(MoveDirection dir, LevelManager manager)
    {
        _moveDirection = dir;
        _levelManager = manager;

        if (arrowShellRenderer != null)
        {
            MaterialPropertyBlock propBlock = new MaterialPropertyBlock();
            arrowShellRenderer.GetPropertyBlock(propBlock);
            // Trimitem forward-ul raycastPoint pentru shader
            propBlock.SetVector(MoveDirectionID, raycastPoint != null ? raycastPoint.forward : transform.forward);
            arrowShellRenderer.SetPropertyBlock(propBlock);
        }
    }

    private void OnMouseUp()
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
        Vector3 origin = raycastPoint != null ? raycastPoint.position : transform.position;
        Vector3 direction = raycastPoint != null ? raycastPoint.forward : transform.forward;
        float maxDistance = 10f;

        return !Physics.Raycast(origin, direction, maxDistance);
    }

    private IEnumerator MoveAndDestroy()
    {
        Vector3 startPosition = transform.position;
        Vector3 direction = raycastPoint != null ? raycastPoint.forward : transform.forward;
        Vector3 endPosition = startPosition + direction * 5f;

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

    private void OnDrawGizmos()
    {
        if (!Application.isPlaying) return;

        Vector3 origin = raycastPoint != null ? raycastPoint.position : transform.position;
        Vector3 direction = raycastPoint != null ? raycastPoint.forward : transform.forward;
        float distance = 10f;

        bool isClear = !Physics.Raycast(origin, direction, distance);
        Gizmos.color = isClear ? Color.green : Color.red;

        Gizmos.DrawRay(origin, direction * distance);
        Gizmos.DrawSphere(origin, 0.05f);
    }
}
