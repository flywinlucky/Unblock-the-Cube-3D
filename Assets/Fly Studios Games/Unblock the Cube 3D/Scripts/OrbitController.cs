using UnityEngine;

// Numele clasei este PascalCase, un standard în C#.
public class OrbitController : MonoBehaviour
{
    // Variabilele publice sunt camelCase și au un Tooltip pentru a explica ce fac în Inspector.
    [Tooltip("Obiectul în jurul căruia se va roti camera.")]
    public Transform target;

    [Tooltip("Cât de departe stă camera de țintă.")]
    public float distance = 5.0f;

    [Tooltip("Viteza de rotație a camerei.")]
    public float rotationSpeed = 1.0f;

    // Unghiurile minime și maxime pe verticală pentru a nu trece camera prin pământ.
    [SerializeField] private float yMinLimit = -20f;
    [SerializeField] private float yMaxLimit = 80f;

    // Variabile private pentru a stoca rotația curentă. Prefixul '_' este o convenție comună.
    private float _x = 0.0f;
    private float _y = 0.0f;

    void Start()
    {
        // Inițializăm unghiurile pe baza rotației curente a camerei.
        Vector3 angles = transform.eulerAngles;
        _x = angles.y;
        _y = angles.x;
    }

    // Folosim LateUpdate pentru a ne asigura că orice mișcare a țintei a fost deja executată.
    void LateUpdate()
    {
        if (target)
        {
            // Verificăm dacă există input de la mouse sau de la touch.
            if (Input.GetMouseButton(0)) // Click stânga
            {
                _x += Input.GetAxis("Mouse X") * rotationSpeed * distance * 0.02f;
                _y -= Input.GetAxis("Mouse Y") * rotationSpeed * 0.02f;
            }
            else if (Input.touchCount > 0) // Atingere pe ecran
            {
                Touch touch = Input.GetTouch(0);
                if (touch.phase == TouchPhase.Moved)
                {
                    _x += touch.deltaPosition.x * rotationSpeed * 0.01f;
                    _y -= touch.deltaPosition.y * rotationSpeed * 0.01f;
                }
            }

            // Limităm rotația pe axa Y.
            _y = ClampAngle(_y, yMinLimit, yMaxLimit);

            // Calculăm noua rotație a camerei.
            Quaternion rotation = Quaternion.Euler(_y, _x, 0);

            // Calculăm poziția camerei.
            Vector3 negDistance = new Vector3(0.0f, 0.0f, -distance);
            Vector3 position = rotation * negDistance + target.position;

            // Aplicăm noile valori de rotație și poziție.
            transform.rotation = rotation;
            transform.position = position;
        }
    }

    // O funcție ajutătoare pentru a limita unghiurile.
    public static float ClampAngle(float angle, float min, float max)
    {
        if (angle < -360F) angle += 360F;
        if (angle > 360F) angle -= 360F;
        return Mathf.Clamp(angle, min, max);
    }
}