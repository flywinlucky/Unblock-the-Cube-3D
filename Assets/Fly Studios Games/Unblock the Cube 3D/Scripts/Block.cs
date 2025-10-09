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
    private float _gridUnitSize; // Stocăm mărimea gridului

    private static readonly int MoveDirectionID = Shader.PropertyToID("_MoveDirection");

    private void Awake()
    {
        _collider = GetComponent<BoxCollider>();

        if (arrowShellRenderer == null)
            Debug.LogError("ArrowShell Renderer nu este asignat în Inspector!", this);
    }

    // MODIFICAT: Acum primim și 'gridUnitSize'
    public void Initialize(MoveDirection dir, LevelManager manager, float gridUnitSize)
    {
        _moveDirection = dir;
        _levelManager = manager;
        _gridUnitSize = gridUnitSize; // Stocăm valoarea

        if (arrowShellRenderer != null)
        {
            MaterialPropertyBlock propBlock = new MaterialPropertyBlock();
            arrowShellRenderer.GetPropertyBlock(propBlock);
            propBlock.SetVector(MoveDirectionID, (Vector4)GetWorldDirection());
            arrowShellRenderer.SetPropertyBlock(propBlock);
        }
    }

    // Folosim OnMouseUp pentru a permite rotirea camerei fără a mișca un bloc
    private void OnMouseUp()
    {
        if (_isMoving) return;

        // --- NOUA LOGICĂ DE CALCULARE A MIȘCĂRII ---
        Vector3 direction = transform.forward;
        RaycastHit hit;
        Vector3 targetPosition;
        bool shouldBeDestroyed = false;

        // Verificăm dacă lovim un alt bloc
        if (Physics.Raycast(transform.position, direction, out hit, 100f))
        {
            // Am lovit un bloc. Ținta este poziția de dinaintea lui.
            targetPosition = hit.transform.position - direction * _gridUnitSize;
        }
        else
        {
            // Nu am lovit nimic. Blocul poate zbura de pe ecran și va fi distrus.
            targetPosition = transform.position + direction * 10f; // Se mișcă mult în afara ecranului
            shouldBeDestroyed = true;
        }

        // Ne mișcăm doar dacă noua poziție este diferită de cea actuală (cu o mică toleranță).
        if (Vector3.Distance(transform.position, targetPosition) > 0.1f)
        {
            _isMoving = true;

            // Dacă blocul va fi distrus, anunțăm managerul imediat.
            if (shouldBeDestroyed)
            {
                _collider.enabled = false;
                _levelManager.OnBlockRemoved(this);
            }

            // Pornim corutina de mișcare cu noii parametri.
            StartCoroutine(Move(targetPosition, shouldBeDestroyed));
        }
    }

    // --- NOUA CORUTINĂ DE MIȘCARE ---
    private IEnumerator Move(Vector3 targetPosition, bool shouldBeDestroyed)
    {
        Vector3 startPosition = transform.position;
        // Durata animației depinde de distanță, pentru o viteză constantă
        float duration = Vector3.Distance(startPosition, targetPosition) * 0.1f;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            transform.position = Vector3.Lerp(startPosition, targetPosition, elapsed / duration);
            elapsed += Time.deltaTime;
            yield return null;
        }

        // Ne asigurăm că ajunge exact la poziția finală
        transform.position = targetPosition;

        if (shouldBeDestroyed)
        {
            Destroy(gameObject);
        }
        else
        {
            // Mișcarea s-a terminat, blocul poate fi apăsat din nou.
            _isMoving = false;
        }
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

    // --- GIZMOS ACTUALIZAT PENTRU A REFLECTA NOUA LOGICĂ ---
    private void OnDrawGizmos()
    {
        if (!Application.isPlaying) return;

        Vector3 direction = transform.forward;
        RaycastHit hit;

        if (Physics.Raycast(transform.position, direction, out hit, 100f))
        {
            // Calea este blocată. Desenăm o linie roșie până la punctul de impact.
            Gizmos.color = Color.red;
            Gizmos.DrawLine(transform.position, hit.point);
        }
        else
        {
            // Calea este liberă. Desenăm o linie verde lungă.
            Gizmos.color = Color.green;
            Gizmos.DrawRay(transform.position, direction * 10f);
        }
    }
}

