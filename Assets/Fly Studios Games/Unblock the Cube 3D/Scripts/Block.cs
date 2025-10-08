using UnityEngine;
using System.Collections;

public class Block : MonoBehaviour
{
    // --- REINTEGRAT: Referința la carcasa pentru săgeți ---
    [Tooltip("Trage aici obiectul copil 'ArrowShell' din Prefab.")]
    public Renderer arrowShellRenderer;

    private MoveDirection _moveDirection;
    private LevelManager _levelManager;
    private bool _isMoving = false;
    private BoxCollider _collider;

    // Optimizare pentru shader
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

        // --- REINTEGRAT: Logica pentru a trimite direcția la shader-ul carcasei ---
        if (arrowShellRenderer != null)
        {
            MaterialPropertyBlock propBlock = new MaterialPropertyBlock();
            arrowShellRenderer.GetPropertyBlock(propBlock);
            // Folosim GetDirectionVector() pentru a trimite direcția corectă la shader
            propBlock.SetVector(MoveDirectionID, (Vector4)GetDirectionVector(true)); // Trimitem vectorul global la shader
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
        // Folosim direcția locală pentru raycast, așa cum ai cerut
        Vector3 directionVector = GetDirectionVector();
        float maxDistance = 10f;

        return !Physics.Raycast(transform.position, directionVector, maxDistance);
    }

    private IEnumerator MoveAndDestroy()
    {
        // Mișcarea se face tot pe direcția locală
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

    // --- MODIFICAT: Funcția a fost adaptată ---
    // Acum poate returna fie vectori locali (pentru mișcare), fie globali (pentru shader).
    private Vector3 GetDirectionVector(bool worldSpace = false)
    {
        if (worldSpace)
        {
            // Returnează vectori globali, necesari pentru shader
            switch (_moveDirection)
            {
                case MoveDirection.Forward: return Vector3.forward;
                case MoveDirection.Back: return Vector3.back;
                case MoveDirection.Up: return Vector3.up;
                case MoveDirection.Down: return Vector3.down;
                case MoveDirection.Left: return Vector3.left;
                case MoveDirection.Right: return Vector3.right;
            }
        }
        else
        {
            // Returnează vectori locali, așa cum ai cerut, pentru mișcare și raycast
            switch (_moveDirection)
            {
                case MoveDirection.Forward: return transform.forward;
                case MoveDirection.Back: return -transform.forward;
                case MoveDirection.Up: return transform.up;
                case MoveDirection.Down: return -transform.up;
                case MoveDirection.Left: return -transform.right;
                case MoveDirection.Right: return transform.right;
            }
        }
        return Vector3.zero;
    }

    private void OnDrawGizmos()
    {
        if (_collider == null) { _collider = GetComponent<BoxCollider>(); }
        // Verificăm dacă suntem în Play Mode pentru a avea o direcție validă
        if (!Application.isPlaying) return;

        bool isClear = IsPathClear();
        Gizmos.color = isClear ? Color.green : Color.red;

        // Gizmo-ul va folosi direcția locală, rotindu-se cu obiectul
        Vector3 direction = GetDirectionVector();
        float distance = 10f;

        Gizmos.DrawRay(transform.position, direction * distance);
    }
}