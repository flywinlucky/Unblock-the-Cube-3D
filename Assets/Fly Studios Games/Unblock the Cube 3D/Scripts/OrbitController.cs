using UnityEngine;

public class OrbitController : MonoBehaviour
{
    // --- Variabile Principale ---
    [Tooltip("Obiectul părinte care conține toate blocurile.")]
    public Transform target;

    // --- Setări de Rotație ---
    [Tooltip("Viteza cu care se rotește obiectul.")]
    public float rotationSpeed = 5.0f;

    // --- Setări de Zoom (Scroll) ---
    [Tooltip("Viteza cu care funcționează zoom-ul camerei.")]
    public float zoomSpeed = 5.0f;
    [Tooltip("Viteza pinch-zoom pentru gesturi cu două degete (mobil).")]
    public float pinchZoomSpeed = 0.02f; // NOU
    [Tooltip("Distanța minimă la care se poate apropia camera.")]
    public float minDistance = 2f;
    [Tooltip("Distanța maximă la care se poate depărta camera.")]
    public float maxDistance = 15f;

    // --- Variabile Private ---
    private float _distance;
    private Vector3 _centerPoint; // NOU: Punctul central în jurul căruia vom roti

    // NOU: smoothing și gestionare touch
    [Header("Touch Settings")]
    [Tooltip("Sensibilitate pentru rotație pe touch.")]
    public float touchRotationSensitivity = 0.2f;
    [Tooltip("Factor de netezire pentru rotație touch (0..1). Valori mici -> mai neted).")]
    [Range(0.01f, 1f)]
    public float touchRotationSmooth = 0.15f;
    [Tooltip("Factor de netezire pentru pinch zoom.")]
    [Range(0.01f, 1f)]
    public float touchZoomSmooth = 0.12f;

    private bool _isTouchRotating = false;
    private bool _isTouchPinching = false;
    private Vector2 _smoothedRotationDelta = Vector2.zero;
    private float _smoothedZoomDelta = 0f;
    private float _lastPinchDistance = 0f;
    private int _activeTouchId = -1; // pentru rotation single touch

    void Start()
    {
        if (target != null)
        {
            // La început, calculăm distanța inițială.
            _distance = Vector3.Distance(transform.position, target.position);

            // NOU: Calculăm centrul geometric al tuturor copiilor.
            CalculateCenterPoint();
        }
    }

    /// <summary>
    /// Calculează punctul central pe baza poziției medii a tuturor copiilor obiectului target.
    /// </summary>
    public void CalculateCenterPoint()
    {
        if (target == null || target.childCount == 0)
        {
            // Dacă nu avem un target sau nu are copii, folosim poziția lui ca fallback.
            _centerPoint = target != null ? target.position : Vector3.zero;
            return;
        }

        Vector3 totalPosition = Vector3.zero;
        foreach (Transform child in target)
        {
            totalPosition += child.position;
        }

        // Centrul este media tuturor pozițiilor
        _centerPoint = totalPosition / target.childCount;
    }

    void LateUpdate()
    {
        if (target)
        {
            // --- MOUSE / TOUCH ROTATION ---
            // Desktop mouse (stânga/dreapta) rămâne la fel
            if (Input.GetMouseButton(0) || Input.GetMouseButton(1))
            {
                float inputX = Input.GetAxis("Mouse X");
                float inputY = Input.GetAxis("Mouse Y");

                target.RotateAround(_centerPoint, Vector3.up, -inputX * rotationSpeed);
                target.RotateAround(_centerPoint, transform.right, inputY * rotationSpeed);

                // Reset touch accumulators când folosim mouse
                _isTouchRotating = false;
                _isTouchPinching = false;
                _smoothedRotationDelta = Vector2.zero;
                _smoothedZoomDelta = 0f;
                _activeTouchId = -1;
            }
            // TOUCH handling
            else if (Input.touchCount > 0)
            {
                // --- PINCH (două degete) ---
                if (Input.touchCount == 2)
                {
                    Touch t0 = Input.GetTouch(0);
                    Touch t1 = Input.GetTouch(1);

                    // Începem pinch: inițializăm distanța pentru a evita saltul
                    float currentPinch = Vector2.Distance(t0.position, t1.position);
                    if (!_isTouchPinching)
                    {
                        _isTouchPinching = true;
                        _isTouchRotating = false;
                        _smoothedZoomDelta = 0f;
                        _lastPinchDistance = currentPinch;
                        _activeTouchId = -1;
                    }
                    else
                    {
                        // delta pinch (positiv = pinch in / zoom out depending pe implementare)
                        float rawDelta = _lastPinchDistance - currentPinch;
                        _lastPinchDistance = currentPinch;

                        // DPI scale ca mobil să se comporte similar pe ecrane diferite
                        float dpiScale = (Screen.dpi > 0f) ? (160f / Screen.dpi) : 1f;
                        // Smooth delta
                        _smoothedZoomDelta = Mathf.Lerp(_smoothedZoomDelta, rawDelta, 1f - Mathf.Exp(-touchZoomSmooth * 60f * Time.deltaTime));

                        // Aplicăm zoom folosind smoothed delta și timp
                        _distance += _smoothedZoomDelta * pinchZoomSpeed * dpiScale * Time.deltaTime;
                        _distance = Mathf.Clamp(_distance, minDistance, maxDistance);

                        Vector3 dir = (transform.position - _centerPoint).normalized;
                        transform.position = _centerPoint + dir * _distance;
                    }
                }
                // --- SINGLE TOUCH (rotație) ---
                else if (Input.touchCount == 1)
                {
                    Touch t = Input.GetTouch(0);

                    // Dacă până acum eram în pinch și s-a eliberat un deget, resetăm acumulatoarele pentru rotație ca să nu sară
                    if (_isTouchPinching)
                    {
                        _isTouchPinching = false;
                        _smoothedZoomDelta = 0f;
                        _smoothedRotationDelta = Vector2.zero;
                        _activeTouchId = -1;
                    }

                    // Când începe touch-ul, setăm id-ul activ și resetăm smoothing
                    if (t.phase == TouchPhase.Began)
                    {
                        _isTouchRotating = true;
                        _smoothedRotationDelta = Vector2.zero;
                        _activeTouchId = t.fingerId;
                    }
                    else if (t.phase == TouchPhase.Moved && _isTouchRotating && t.fingerId == _activeTouchId)
                    {
                        // aplicăm sensibilitate și DPI scaling
                        float dpiScale = (Screen.dpi > 0f) ? (160f / Screen.dpi) : 1f;
                        Vector2 rawDelta = t.deltaPosition * touchRotationSensitivity * dpiScale;

                        // netezim delta (exponential-like smoothing)
                        float smoothFactor = 1f - Mathf.Exp(-touchRotationSmooth * 60f * Time.deltaTime);
                        _smoothedRotationDelta = Vector2.Lerp(_smoothedRotationDelta, rawDelta, smoothFactor);

                        // Aplicăm rotația folosind Time.deltaTime pentru stabilitate framerate
                        float rotX = -_smoothedRotationDelta.x * rotationSpeed * Time.deltaTime;
                        float rotY = _smoothedRotationDelta.y * rotationSpeed * Time.deltaTime;

                        target.RotateAround(_centerPoint, Vector3.up, rotX);
                        target.RotateAround(_centerPoint, transform.right, rotY);
                    }
                    else if (t.phase == TouchPhase.Ended || t.phase == TouchPhase.Canceled)
                    {
                        _isTouchRotating = false;
                        _activeTouchId = -1;
                        _smoothedRotationDelta = Vector2.zero;
                    }
                }
            }
            else
            {
                // nici mouse nici touch -> resetăm stările touch
                _isTouchRotating = false;
                _isTouchPinching = false;
                _activeTouchId = -1;
                _smoothedRotationDelta = Vector2.zero;
                _smoothedZoomDelta = 0f;
            }
        }

        // --- Preluarea Input-ului pentru Zoom-ul Camerei ---
        // Pinch-to-zoom (mobil) - două degete
        if (Input.touchCount == 2)
        {
            Touch t0 = Input.GetTouch(0);
            Touch t1 = Input.GetTouch(1);

            // pozițiile anterioare ale celor două atingeri
            Vector2 t0Prev = t0.position - t0.deltaPosition;
            Vector2 t1Prev = t1.position - t1.deltaPosition;

            float prevMagnitude = (t0Prev - t1Prev).magnitude;
            float currentMagnitude = (t0.position - t1.position).magnitude;

            float deltaMagnitudeDiff = prevMagnitude - currentMagnitude;

            _distance += deltaMagnitudeDiff * pinchZoomSpeed;
            _distance = Mathf.Clamp(_distance, minDistance, maxDistance);

            Vector3 dir = (transform.position - _centerPoint).normalized;
            transform.position = _centerPoint + dir * _distance;
        }
        else
        {
            // Scroll wheel / desktop zoom fallback
            float scroll = Input.GetAxis("Mouse ScrollWheel");
            if (scroll != 0.0f)
            {
                _distance -= scroll * zoomSpeed;
                _distance = Mathf.Clamp(_distance, minDistance, maxDistance);

                // Asigurăm poziția corectă față de centrul calculat.
                Vector3 direction = (transform.position - _centerPoint).normalized;
                transform.position = _centerPoint + direction * _distance;
            }
        }
    }
}