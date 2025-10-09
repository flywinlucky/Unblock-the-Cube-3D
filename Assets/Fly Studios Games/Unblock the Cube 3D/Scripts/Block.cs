// Block.cs
using UnityEngine;
using System.Collections;

public class Block : MonoBehaviour
{
    [Tooltip("Trage aici obiectul copil 'ArrowShell' din Prefab.")]
    public Renderer arrowShellRenderer;

    [Header("Animation Settings")]
    [Tooltip("Durata animației de dispariție (scale out) în secunde.")]
    public float dissolveDuration = 0.2f;

    private MoveDirection _moveDirection;
    private LevelManager _levelManager;
    private bool _isMoving = false;
    private BoxCollider _collider;
    private float _gridUnitSize;

    private static readonly int MoveDirectionID = Shader.PropertyToID("_MoveDirection");
    private bool _isShaking = false;

    private void Awake()
    {
        _collider = GetComponent<BoxCollider>();
        if (arrowShellRenderer == null)
            Debug.LogError("ArrowShell Renderer nu este asignat în Inspector!", this);
    }

    public void Initialize(MoveDirection dir, LevelManager manager, float gridUnitSize)
    {
        _moveDirection = dir;
        _levelManager = manager;
        _gridUnitSize = gridUnitSize;

        if (arrowShellRenderer != null)
        {
            MaterialPropertyBlock propBlock = new MaterialPropertyBlock();
            arrowShellRenderer.GetPropertyBlock(propBlock);
            propBlock.SetVector(MoveDirectionID, (Vector4)GetWorldDirection());
            arrowShellRenderer.SetPropertyBlock(propBlock);
        }
    }

    private void OnMouseUpAsButton()
    {
        if (_isMoving) return;

        Vector3 direction = transform.forward;
        RaycastHit hit;
        Vector3 targetPosition;
        bool shouldBeDestroyed = false;

        if (Physics.Raycast(transform.position, direction, out hit, 100f))
        {
            targetPosition = hit.transform.position - direction * _gridUnitSize;
            if (Vector3.Distance(transform.position, targetPosition) < 0.1f)
            {
                StartCoroutine(ShakeScale());
                return;
            }
        }
        else
        {
            targetPosition = transform.position + direction * 10f;
            shouldBeDestroyed = true;
        }

        if (Vector3.Distance(transform.position, targetPosition) > 0.1f)
        {
            _isMoving = true;
            if (shouldBeDestroyed)
            {
                _collider.enabled = false;
                _levelManager.OnBlockRemoved(this);
            }
            StartCoroutine(MoveWithDamping(targetPosition, shouldBeDestroyed));
        }
        else
        {
            StartCoroutine(ShakeScale());
        }
    }

    // ▼▼▼ MODIFICARE CHEIE AICI ▼▼▼
    private IEnumerator MoveWithDamping(Vector3 targetPosition, bool shouldBeDestroyed)
    {
        Vector3 startPosition = transform.position;
        float duration = Vector3.Distance(startPosition, targetPosition) * 0.1f;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            transform.position = Vector3.Lerp(startPosition, targetPosition, Mathf.SmoothStep(0, 1, elapsed / duration));
            elapsed += Time.deltaTime;
            yield return null;
        }

        transform.position = targetPosition;

        // Dacă blocul trebuie distrus, pornim animația de dizolvare.
        if (shouldBeDestroyed)
        {
            // Așteptăm finalizarea noii animații, care se va ocupa și de distrugerea obiectului.
            yield return StartCoroutine(ScaleOutAndDestroy());
        }
        // Altfel, dacă doar lovește un alt bloc, facem animația de impact.
        else
        {
            yield return StartCoroutine(ImpactBounce());
            _isMoving = false;
        }
    }

    // ▼▼▼ FUNCȚIE NOUĂ ▼▼▼
    /// <summary>
    /// Animație lină de micșorare a scării până la zero, urmată de distrugerea obiectului.
    /// </summary>
    private IEnumerator ScaleOutAndDestroy()
    {
        Vector3 originalScale = transform.localScale;
        Vector3 targetScale = Vector3.zero;
        float elapsed = 0f;

        while (elapsed < dissolveDuration)
        {
            // Interpolăm scara de la cea originală la zero
            transform.localScale = Vector3.Lerp(originalScale, targetScale, elapsed / dissolveDuration);
            elapsed += Time.deltaTime;
            yield return null;
        }

        // La final, distrugem obiectul
        Destroy(gameObject);
    }

    // --- Restul scriptului rămâne neschimbat ---

    private IEnumerator ShakeScale()
    {
        if (_isShaking) yield break;
        _isShaking = true;
        Vector3 originalScale = transform.localScale;
        Vector3 targetScale = originalScale * 1.15f;
        float duration = 0.1f;
        float elapsed = 0f;
        while (elapsed < duration)
        {
            transform.localScale = Vector3.Lerp(originalScale, targetScale, elapsed / duration);
            elapsed += Time.deltaTime;
            yield return null;
        }
        elapsed = 0f;
        while (elapsed < duration)
        {
            transform.localScale = Vector3.Lerp(targetScale, originalScale, elapsed / duration);
            elapsed += Time.deltaTime;
            yield return null;
        }
        transform.localScale = originalScale;
        _isShaking = false;
    }

    private IEnumerator ImpactBounce()
    {
        if (_isShaking) yield break;
        _isShaking = true;
        Vector3 originalPos = transform.position;
        Vector3 bouncePos = originalPos - transform.forward * 0.05f;
        float duration = 0.08f;
        float elapsed = 0f;
        while (elapsed < duration)
        {
            transform.position = Vector3.Lerp(originalPos, bouncePos, Mathf.SmoothStep(0, 1, elapsed / duration));
            elapsed += Time.deltaTime;
            yield return null;
        }
        elapsed = 0f;
        while (elapsed < duration)
        {
            transform.position = Vector3.Lerp(bouncePos, originalPos, Mathf.SmoothStep(0, 1, elapsed / duration));
            elapsed += Time.deltaTime;
            yield return null;
        }
        transform.position = originalPos;
        _isShaking = false;
    }

    private Vector3 GetWorldDirection()
    {
        switch (_moveDirection)
        {
            case MoveDirection.Forward: return Vector3.forward;
            case MoveDirection.Back: return Vector3.back;
            case MoveDirection.Up: return Vector3.up;
            case MoveDirection.Down: return Vector3.down;
            case MoveDirection.Left: return Vector3.left;
            case MoveDirection.Right: return Vector3.right;
        }
        return Vector3.zero;
    }

    private void OnDrawGizmos()
    {
        if (!Application.isPlaying) return;
        Vector3 direction = transform.forward;
        RaycastHit hit;
        if (Physics.Raycast(transform.position, direction, out hit, 100f))
        {
            Gizmos.color = Color.red;
            Gizmos.DrawLine(transform.position, hit.point);
        }
        else
        {
            Gizmos.color = Color.green;
            Gizmos.DrawRay(transform.position, direction * 10f);
        }
    }
}