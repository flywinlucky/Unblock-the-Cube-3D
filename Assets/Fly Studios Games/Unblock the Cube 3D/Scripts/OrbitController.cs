using UnityEngine;

// Am redenumit clasa pentru a reflecta noua funcționalitate.
public class OrbitController : MonoBehaviour
{
    // --- Variabile Principale ---
    [Tooltip("Obiectul care va fi rotit.")]
    public Transform target;

    // --- Setări de Rotație ---
    [Tooltip("Viteza cu care se rotește obiectul.")]
    public float rotationSpeed = 5.0f;

    // --- Setări de Zoom (Scroll) ---
    [Tooltip("Viteza cu care funcționează zoom-ul camerei.")]
    public float zoomSpeed = 5.0f;
    [Tooltip("Distanța minimă la care se poate apropia camera.")]
    public float minDistance = 2f;
    [Tooltip("Distanța maximă la care se poate depărta camera.")]
    public float maxDistance = 15f;

    // Variabilă privată pentru a stoca distanța curentă
    private float _distance;

    void Start()
    {
        // La început, calculăm și stocăm distanța inițială dintre cameră și țintă.
        if (target != null)
        {
            _distance = Vector3.Distance(transform.position, target.position);
        }
    }

    void LateUpdate()
    {
        if (target)
        {
            // --- Preluarea Input-ului pentru Rotația Obiectului ---
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

                // Aplicăm rotația direct pe OBIECT (target), nu pe cameră.
                // Rotația pe orizontală se face în jurul axei Y a lumii (sus/jos).
                target.Rotate(Vector3.up, -inputX * rotationSpeed, Space.World);
                // Rotația pe verticală se face în jurul axei X a CAMEREI (dreapta/stânga ei).
                target.Rotate(transform.right, inputY * rotationSpeed, Space.World);
            }

            // --- Preluarea Input-ului pentru Zoom-ul Camerei ---
            float scroll = Input.GetAxis("Mouse ScrollWheel");
            if (scroll != 0.0f)
            {
                // Modificăm distanța stocată
                _distance -= scroll * zoomSpeed;
                // Limităm distanța între valorile minime și maxime
                _distance = Mathf.Clamp(_distance, minDistance, maxDistance);

                // Recalculăm poziția camerei pentru a reflecta noul zoom.
                // Direcția este de la țintă spre cameră.
                Vector3 direction = (transform.position - target.position).normalized;
                transform.position = target.position + direction * _distance;
            }
        }
    }
}