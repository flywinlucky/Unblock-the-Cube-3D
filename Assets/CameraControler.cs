using UnityEngine;

/// <summary>
/// Controlează rotația unui obiect "target" în jurul centrului său geometric și zoom-ul camerei.
/// Rotația se face direct pe obiectul target, iar camera își menține poziția, uitându-se mereu la centru.
/// </summary>
public class CameraControler : MonoBehaviour
{
    [Tooltip("Obiectul părinte care va fi rotit.")]
    public Transform target;

    [Header("Rotation Settings")]
    [Tooltip("Viteza de rotație a obiectului cu mouse-ul.")]
    public float rotationSpeed = 5.0f;
    [Tooltip("Cât de lină să fie mișcarea de rotație (valori mai mici = mai rapid).")]
    [Range(0.01f, 0.5f)]
    public float rotationDamping = 0.05f;

    [Header("Zoom Settings")]
    [Tooltip("Viteza cu care funcționează zoom-ul.")]
    public float zoomSpeed = 10.0f;
    [Tooltip("Cât de lin să fie zoom-ul (valori mai mici = mai rapid).")]
    [Range(0.01f, 0.5f)]
    public float zoomDamping = 0.1f;
    [Tooltip("Distanța minimă la care se poate apropia camera.")]
    public float minDistance = 3f;
    [Tooltip("Distanța maximă la care se poate depărta camera.")]
    public float maxDistance = 15f;

    // --- Variabile Private ---
    private Vector3 _centerPoint;
    private float _currentDistance;
    private float _desiredDistance;

    // Variabile pentru rotație lină
    private Vector2 _rotationInput;
    private Vector2 _smoothVelocity;

    void Start()
    {
        if (target == null)
        {
            Debug.LogError("Target-ul nu a fost setat în scriptul CameraControler.", this);
            this.enabled = false;
            return;
        }

        // 1. Calculăm centrul geometric al copiilor
        CalculateCenterPoint();

        // 2. Inițializăm distanța camerei
        _currentDistance = Vector3.Distance(transform.position, _centerPoint);
        _desiredDistance = _currentDistance;
    }

    void LateUpdate()
    {
        if (target)
        {
            HandleRotation();
            HandleZoom();

            // La final, ne asigurăm că, indiferent de zoom, camera se uită mereu la centru
            transform.LookAt(_centerPoint);
        }
    }

    /// <summary>
    /// Calculează punctul central pe baza poziției medii a tuturor copiilor obiectului target.
    /// </summary>
    private void CalculateCenterPoint()
    {
        if (target.childCount == 0)
        {
            _centerPoint = target.position;
            Debug.LogWarning("Target-ul nu are copii. Se folosește pivotul propriu ca centru de rotație.", target);
            return;
        }

        Vector3 totalPosition = Vector3.zero;
        foreach (Transform child in target)
        {
            totalPosition += child.position;
        }
        _centerPoint = totalPosition / target.childCount;
    }

    /// <summary>
    /// Prelucrează input-ul de la mouse și rotește obiectul target.
    /// </summary>
    private void HandleRotation()
    {
        // Preluăm input-ul brut de la mouse doar dacă un buton este apăsat
        Vector2 rawInput = Vector2.zero;
        if (Input.GetMouseButton(0) || Input.GetMouseButton(1))
        {
            // Folosim direct GetAxis pentru a fi independent de framerate
            rawInput = new Vector2(Input.GetAxis("Mouse X"), Input.GetAxis("Mouse Y"));
        }

        // Netezim input-ul folosind SmoothDamp pentru o mișcare fluidă
        _rotationInput = Vector2.SmoothDamp(_rotationInput, rawInput, ref _smoothVelocity, rotationDamping);

        // Aplicăm rotația pe target în jurul centrului calculat
        if (_rotationInput.magnitude > 0.001f)
        {
            // Rotație orizontală (stânga/dreapta) în jurul axei globale Y
            target.RotateAround(_centerPoint, Vector3.up, -_rotationInput.x * rotationSpeed);

            // Rotație verticală (sus/jos) în jurul axei 'right' a camerei
            target.RotateAround(_centerPoint, transform.right, _rotationInput.y * rotationSpeed);
        }
    }

    /// <summary>
    /// Prelucrează input-ul de la scroll și ajustează distanța camerei (zoom).
    /// </summary>
    private void HandleZoom()
    {
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (Mathf.Abs(scroll) > 0.01f)
        {
            _desiredDistance -= scroll * zoomSpeed;
            _desiredDistance = Mathf.Clamp(_desiredDistance, minDistance, maxDistance);
        }

        _currentDistance = Mathf.Lerp(_currentDistance, _desiredDistance, zoomDamping);

        // Obținem direcția de la centru spre cameră
        Vector3 directionFromCenter = (transform.position - _centerPoint).normalized;

        // Poziția corectă este la centrul, plus direcția înmulțită cu distanța
        transform.position = _centerPoint + directionFromCenter * _currentDistance;
    }
}

