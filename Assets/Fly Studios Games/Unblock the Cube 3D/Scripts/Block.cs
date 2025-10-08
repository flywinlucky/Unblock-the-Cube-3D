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
            Debug.LogError("ArrowShell Renderer nu este asignat în Inspector!", this);
    }

    public void Initialize(MoveDirection dir, LevelManager manager)
    {
        _moveDirection = dir;
        _levelManager = manager;

        if (arrowShellRenderer != null)
        {
            MaterialPropertyBlock propBlock = new MaterialPropertyBlock();
            arrowShellRenderer.GetPropertyBlock(propBlock);
            // Trimitem direcția GLOBALĂ la shader pentru ca săgețile să fie orientate corect
            propBlock.SetVector(MoveDirectionID, (Vector4)GetWorldDirection());
            arrowShellRenderer.SetPropertyBlock(propBlock);
        }
    }

    // Folosim OnMouseUp pentru a permite rotirea camerei fără a mișca un bloc
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
        // Raycast-ul pornește din centrul obiectului, în direcția sa "înainte" locală
        return !Physics.Raycast(transform.position, transform.forward, 10f);
    }

    private IEnumerator MoveAndDestroy()
    {
        Vector3 startPosition = transform.position;
        // Mișcarea se face în direcția "înainte" locală a obiectului
        Vector3 endPosition = startPosition + transform.forward * 5f;

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

    // Funcție separată pentru a obține direcția globală, necesară pentru shader
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

        // Gizmo-ul afișează acum raza locală, care se rotește cu obiectul
        bool isClear = !Physics.Raycast(transform.position, transform.forward, 10f);
        Gizmos.color = isClear ? Color.green : Color.red;

        Gizmos.DrawRay(transform.position, transform.forward * 10f);
    }
}

