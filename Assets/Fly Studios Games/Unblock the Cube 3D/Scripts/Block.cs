using UnityEngine;
using System.Collections;

public class Block : MonoBehaviour
{
    [Tooltip("Trage aici obiectul copil 'ArrowShell' din Prefab.")]
    public Renderer arrowShellRenderer;

    private MoveDirection _moveDirection;
    private LevelManager _levelManager;
    private bool _isMoving = false;
    private BoxCollider _collider;

    private static readonly int MoveDirectionID = Shader.PropertyToID("_MoveDirection");

    private void Awake()
    {
        _collider = GetComponent<BoxCollider>();
        if (arrowShellRenderer == null)
        {
            Debug.LogError("ArrowShell Renderer nu este asignat în Inspector!", this.gameObject);
        }
    }

    public void Initialize(MoveDirection dir, LevelManager manager)
    {
        _moveDirection = dir;
        _levelManager = manager;

        if (arrowShellRenderer != null)
        {
            MaterialPropertyBlock propBlock = new MaterialPropertyBlock();
            arrowShellRenderer.GetPropertyBlock(propBlock);
            // Acum trimitem mereu vectorul global la shader, ceea ce este corect.
            propBlock.SetVector(MoveDirectionID, (Vector4)GetDirectionVector());
            arrowShellRenderer.SetPropertyBlock(propBlock);
        }
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
        // Raycast-ul folosește acum direcția globală, consistentă.
        Vector3 directionVector = GetDirectionVector();
        float maxDistance = 10f;

        return !Physics.Raycast(transform.position, directionVector, maxDistance);
    }

    private IEnumerator MoveAndDestroy()
    {
        // Mișcarea se face pe direcția globală.
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

    // --- SIMPLIFICAT: Funcția returnează acum DOAR vectori globali ---
    // Aceasta devine singura "sursă de adevăr" pentru direcție.
    private Vector3 GetDirectionVector()
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

        bool isClear = IsPathClear();
        Gizmos.color = isClear ? Color.green : Color.red;

        // Gizmo-ul va afișa acum direcția globală, reală, de verificare.
        Vector3 direction = GetDirectionVector();
        float distance = 10f;

        Gizmos.DrawRay(transform.position, direction * distance);
    }
}