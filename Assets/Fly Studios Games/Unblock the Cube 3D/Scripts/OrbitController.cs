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
            // --- Preluarea Input-ului pentru Rotația Obiectului ---
            // Acceptăm atât click stânga (0) cât și click dreapta (1) pentru drag/rotate, plus touch
            if (Input.GetMouseButton(0) || Input.GetMouseButton(1) || (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Moved))
            {
                float inputX = 0f;
                float inputY = 0f;

                // Mouse (stânga sau dreapta)
                if (Input.GetMouseButton(0) || Input.GetMouseButton(1))
                {
                    inputX = Input.GetAxis("Mouse X");
                    inputY = Input.GetAxis("Mouse Y");
                }
                // Touch
                else
                {
                    Touch touch = Input.GetTouch(0);
                    inputX = touch.deltaPosition.x * 0.1f;
                    inputY = touch.deltaPosition.y * 0.1f;
                }

                // Rotație în jurul centrului calculat (același comportament pentru ambele butoane)
                target.RotateAround(_centerPoint, Vector3.up, -inputX * rotationSpeed);
                target.RotateAround(_centerPoint, transform.right, inputY * rotationSpeed);
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