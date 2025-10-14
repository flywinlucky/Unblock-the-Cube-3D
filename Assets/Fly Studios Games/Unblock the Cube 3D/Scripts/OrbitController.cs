using UnityEngine;

public class OrbitController : MonoBehaviour
{
    // --- Variabile Principale ---
    [Tooltip("Obiectul părinte care conține toate blocurile.")]
    public Transform target;

    // --- Setări de Rotație ---
    [Header("Rotation Settings")]
    [Tooltip("Viteza de rotație pentru mouse.")]
    public float mouseRotationSpeed = 5.0f;
    [Tooltip("Viteza de rotație pentru touch (ajusteaz-o independent pentru mobil).")]
    public float touchRotationSpeed = 0.5f; // NOU: Sensibilitate separată pentru touch
    [Tooltip("Cât de lină să fie rotația (0 = instant, 1 = nu se mișcă).")]
    [Range(0.01f, 0.5f)]
    public float rotationDamping = 0.1f;

    // --- Setări de Zoom (Scroll) ---
    [Header("Zoom Settings")]
    [Tooltip("Viteza zoom-ului cu scroll-ul mouse-ului.")]
    public float zoomSpeed = 5.0f;
    [Tooltip("Viteza pinch-zoom pentru gesturi cu două degete (mobil).")]
    public float pinchZoomSpeed = 0.02f;
    [Tooltip("Cât de lin să fie zoom-ul (0 = instant, 1 = nu se mișcă).")]
    [Range(0.01f, 0.5f)]
    public float zoomDamping = 0.15f;
    [Tooltip("Distanța minimă la care se poate apropia camera.")]
    public float minDistance = 2f;
    [Tooltip("Distanța maximă la care se poate depărta camera.")]
    public float maxDistance = 15f;

    // --- Variabile Private pentru funcționare ---
    private Vector3 _centerPoint;
    private float _desiredDistance;
    private float _currentDistance;
    private Quaternion _desiredRotation;
    private Vector3 _lastMousePosition; // NOU: Pentru a preveni salturi la primul click

    void Start()
    {
        if (target == null)
        {
            Debug.LogError("Target object is not assigned in the OrbitController script.");
            this.enabled = false;
            return;
        }

        CalculateCenterPoint();

        // Inițializăm variabilele
        _currentDistance = Vector3.Distance(transform.position, _centerPoint);
        _desiredDistance = _currentDistance;
        _desiredRotation = target.rotation;
    }

    /// <summary>
    /// Calculează punctul central pe baza poziției medii a tuturor copiilor obiectului target.
    /// </summary>
    public void CalculateCenterPoint()
    {
        if (target == null || target.childCount == 0)
        {
            _centerPoint = target != null ? target.position : Vector3.zero;
            return;
        }

        Vector3 totalPosition = Vector3.zero;
        foreach (Transform child in target)
        {
            totalPosition += child.position;
        }
        _centerPoint = totalPosition / target.childCount;
    }

    void LateUpdate()
    {
        if (target)
        {
            HandleInput();
            ApplyTransformations();
        }
    }

    /// <summary>
    /// Prelucrează input-urile de la utilizator (mouse și touch) pentru a determina rotația și zoom-ul dorite.
    /// </summary>
    private void HandleInput()
    {
        // --- GESTURI TOUCH (MOBIL) ---
        if (Input.touchCount > 0)
        {
            // 1. Pinch-to-Zoom (două degete) - PRIORITAR
            if (Input.touchCount == 2)
            {
                Touch t0 = Input.GetTouch(0);
                Touch t1 = Input.GetTouch(1);

                Vector2 t0Prev = t0.position - t0.deltaPosition;
                Vector2 t1Prev = t1.position - t1.deltaPosition;

                float prevMagnitude = (t0Prev - t1Prev).magnitude;
                float currentMagnitude = (t0.position - t1.position).magnitude;
                float deltaMagnitudeDiff = prevMagnitude - currentMagnitude;

                _desiredDistance += deltaMagnitudeDiff * pinchZoomSpeed;
            }
            // 2. Rotație cu un singur deget
            else if (Input.touchCount == 1 && Input.GetTouch(0).phase == TouchPhase.Moved)
            {
                Touch touch = Input.GetTouch(0);
                float inputX = touch.deltaPosition.x;
                float inputY = touch.deltaPosition.y;

                // Calculăm rotația dorită
                Quaternion rotationX = Quaternion.AngleAxis(-inputX * touchRotationSpeed, Vector3.up);
                Quaternion rotationY = Quaternion.AngleAxis(inputY * touchRotationSpeed, transform.right);

                _desiredRotation = rotationX * rotationY * _desiredRotation;
            }
        }
        // --- INPUT MOUSE (PC) ---
        else
        {
            // 1. Rotație cu mouse-ul
            if (Input.GetMouseButton(0) || Input.GetMouseButton(1))
            {
                 // Prevenim saltul la primul click
                if(Input.GetMouseButtonDown(0) || Input.GetMouseButtonDown(1)) {
                    _lastMousePosition = Input.mousePosition;
                }
                
                Vector3 delta = Input.mousePosition - _lastMousePosition;
                float inputX = delta.x;
                float inputY = delta.y;

                // Calculăm rotația dorită
                Quaternion rotationX = Quaternion.AngleAxis(-inputX * mouseRotationSpeed * Time.deltaTime, Vector3.up);
                Quaternion rotationY = Quaternion.AngleAxis(inputY * mouseRotationSpeed * Time.deltaTime, transform.right);
                
                _desiredRotation = rotationX * rotationY * _desiredRotation;
                _lastMousePosition = Input.mousePosition;
            }

            // 2. Zoom cu scroll
            float scroll = Input.GetAxis("Mouse ScrollWheel");
            if (scroll != 0.0f)
            {
                _desiredDistance -= scroll * zoomSpeed;
            }
        }

        // Aplicăm limitele pentru distanță
        _desiredDistance = Mathf.Clamp(_desiredDistance, minDistance, maxDistance);
    }

    /// <summary>
    /// Aplică lin (cu damping) rotația și zoom-ul calculate în HandleInput().
    /// </summary>
    private void ApplyTransformations()
    {
        // Aplicăm rotația cu Slerp pentru o mișcare lină
        target.rotation = Quaternion.Slerp(target.rotation, _desiredRotation, rotationDamping);

        // Aplicăm zoom-ul cu Lerp pentru o mișcare lină
        _currentDistance = Mathf.Lerp(_currentDistance, _desiredDistance, zoomDamping);

        // Actualizăm poziția camerei pe baza noii distanțe și a punctului central
        Vector3 direction = (transform.position - _centerPoint).normalized;
        transform.position = _centerPoint + direction * _currentDistance;

        // Ne asigurăm că camera se uită mereu spre punctul central
        transform.LookAt(_centerPoint);
    }
}
