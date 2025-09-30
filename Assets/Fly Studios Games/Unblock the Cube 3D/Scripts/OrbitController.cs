using UnityEngine;

public class OrbitController : MonoBehaviour
{
    // --- Variabile Principale ---
    [Tooltip("Obiectul în jurul căruia se va roti camera.")]
    public Transform target;

    // --- Setări de Rotație ---
    [Tooltip("Viteza de rotație a camerei.")]
    public float rotationSpeed = 1.0f;

    // --- Setări de Zoom (Scroll) ---
    [Tooltip("Distanța curentă față de țintă.")]
    public float distance = 5.0f;
    [Tooltip("Distanța minimă la care ne putem apropia.")]
    public float minDistance = 2f;
    [Tooltip("Distanța maximă la care ne putem depărta.")]
    public float maxDistance = 15f;
    [Tooltip("Viteza cu care funcționează zoom-ul.")]
    public float zoomSpeed = 5.0f;

    // Variabile private pentru a stoca rotația curentă.
    private float _x = 0.0f;
    private float _y = 0.0f;

    void Start()
    {
        // Inițializăm unghiurile pe baza rotației curente a camerei.
        Vector3 angles = transform.eulerAngles;
        _x = angles.y;
        _y = angles.x;
    }

    void LateUpdate()
    {
        if (target)
        {
            // --- Preluarea Input-ului pentru Rotație ---
            if (Input.GetMouseButton(0) || (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Moved))
            {
                float inputX = 0f;
                float inputY = 0f;

                if (Input.GetMouseButton(0))
                {
                    inputX = Input.GetAxis("Mouse X");
                    inputY = Input.GetAxis("Mouse Y");
                }
                else
                {
                    Touch touch = Input.GetTouch(0);
                    inputX = touch.deltaPosition.x * 0.1f;
                    inputY = touch.deltaPosition.y * 0.1f;
                }

                _x += inputX * rotationSpeed;
                _y -= inputY * rotationSpeed;
            }

            // --- Preluarea Input-ului pentru Zoom ---
            float scroll = Input.GetAxis("Mouse ScrollWheel");
            distance -= scroll * zoomSpeed;
            distance = Mathf.Clamp(distance, minDistance, maxDistance);

            // --- Aplicarea Directă a Poziției și Rotației ---
            // Calculăm noua rotație a camerei.
            Quaternion rotation = Quaternion.Euler(_y, _x, 0);

            // Calculăm noua poziție a camerei.
            Vector3 negDistance = new Vector3(0.0f, 0.0f, -distance);
            Vector3 position = rotation * negDistance + target.position;

            // Aplicăm instantaneu noile valori.
            transform.rotation = rotation;
            transform.position = position;
        }
    }
}