using System.Collections;
using UnityEngine;

/// <summary>
/// Controlează rotația unui obiect "target" în jurul centrului său geometric și zoom-ul camerei.
/// Include o funcție pentru a încadra automat obiectul în vizorul camerei.
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
    public float maxDistance = 50f;
    [Tooltip("Marginea lăsată în jurul obiectului la încadrare (ex: 1.2 = 20% buffer).")]
    public float frameBuffer = 1.5f;

    // --- Variabile Private ---
    private Vector3 _centerPoint;
    private float _currentDistance;
    private float _desiredDistance;
    private Camera _mainCamera;

    // Variabile pentru rotație lină
    private Vector2 _rotationInput;
    private Vector2 _smoothVelocity;

    // Schimbat în IEnumerator pentru a ne asigura că totul este inițializat corect
    IEnumerator Start()
    {
        _mainCamera = Camera.main;
        if (target == null || _mainCamera == null)
        {
            Debug.LogError("Target-ul sau Camera principală nu au fost setate.", this);
            this.enabled = false;
            yield break; // Oprește execuția dacă ceva lipsește
        }

        // Așteptăm un singur frame pentru a permite tuturor obiectelor (cuburilor)
        // să se inițializeze înainte de a calcula încadrarea.
        yield return new WaitForEndOfFrame();

        // Acum apelăm funcția de încadrare, care va acționa instantaneu.
        FrameTarget();
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

        // Apasă F pentru a re-încadra instantaneu obiectul
        if (Input.GetKeyDown(KeyCode.F))
        {
            FrameTarget();
        }
    }

    /// <summary>
    /// Calculează și aplică INSTANTANEU poziția optimă a camerei pentru a încadra obiectul.
    /// </summary>
    public void FrameTarget()
    {
        // Pasul 1: Calculează "cutia" invizibilă (Bounds) care înconjoară toți copiii
        Bounds totalBounds = new Bounds();
        Renderer[] renderers = target.GetComponentsInChildren<Renderer>();

        if (renderers.Length == 0)
        {
            _centerPoint = target.position;
            Debug.LogWarning("Target-ul nu are componente Renderer. Încadrarea poate fi imprecisă.", target);
            totalBounds = new Bounds(target.position, Vector3.one); // O cutie de rezervă
        }
        else
        {
            totalBounds = renderers[0].bounds;
            foreach (Renderer rend in renderers)
            {
                totalBounds.Encapsulate(rend.bounds);
            }
        }

        // Actualizăm punctul central pentru rotație, bazat pe centrul real al obiectului compus
        _centerPoint = totalBounds.center;

        // Pasul 2: Calculează distanța necesară pentru a vedea întreaga "cutie"
        float objectSize = Mathf.Max(totalBounds.size.x, totalBounds.size.y); // Luăm cea mai mare dimensiune (lățime sau înălțime)
        float cameraView = 2.0f * Mathf.Tan(0.5f * _mainCamera.fieldOfView * Mathf.Deg2Rad); // Cât vede camera la 1 unitate distanță
        float distanceToFit = (objectSize / _mainCamera.aspect) / cameraView; // Calculăm distanța

        // Adăugăm un buffer pentru a nu fi exact la margine, folosind variabila publică
        float finalDistance = distanceToFit * frameBuffer;

        // Pasul 3: Setăm noua distanță și o aplicăm IMEDIAT
        float targetDistance = Mathf.Clamp(finalDistance, minDistance, maxDistance);

        // Setăm ambele distanțe (curentă și dorită) la aceeași valoare pentru a anula orice efect de Lerp
        _desiredDistance = targetDistance;
        _currentDistance = targetDistance;

        // Poziționăm camera la distanța corectă de noul centru,
        // menținând direcția curentă a camerei. LookAt() din LateUpdate va corecta unghiul.
        transform.position = _centerPoint - transform.forward * _currentDistance;
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
            rawInput = new Vector2(Input.GetAxis("Mouse X"), Input.GetAxis("Mouse Y"));
        }

        // Netezim input-ul folosind SmoothDamp pentru o mișcare fluidă
        _rotationInput = Vector2.SmoothDamp(_rotationInput, rawInput, ref _smoothVelocity, rotationDamping);

        // Aplicăm rotația pe target în jurul centrului calculat
        if (_rotationInput.magnitude > 0.001f)
        {
            target.RotateAround(_centerPoint, Vector3.up, -_rotationInput.x * rotationSpeed);
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

        // Zoom-ul cu scroll-ul rămâne lin, dar încadrarea este instantanee
        _currentDistance = Mathf.Lerp(_currentDistance, _desiredDistance, zoomDamping);

        Vector3 directionFromCenter = (transform.position - _centerPoint).normalized;
        if (directionFromCenter == Vector3.zero) directionFromCenter = -transform.forward;
        transform.position = _centerPoint + directionFromCenter * _currentDistance;
    }
}

