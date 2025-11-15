using UnityEngine;

public class Player : MonoBehaviour
{
    public Camera cam;
    public float zDepth = 0f;

    private Vector3 worldPos;

    void Update()
    {
        // Convertim mouse-ul în poziție din lume
        Vector3 mouse = Input.mousePosition;
        mouse.z = -cam.transform.position.z + zDepth;
        worldPos = cam.ScreenToWorldPoint(mouse);

        // Direcția din player către mouse
        Vector2 direction = worldPos - transform.position;

        // Calculăm unghiul
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

        // Rotește doar pe axa Z
        transform.rotation = Quaternion.Euler(0f, 0f, angle);
    }

    void OnDrawGizmos()
    {
        if (cam == null)
            cam = Camera.main;

        Gizmos.color = Color.red;
        Gizmos.DrawSphere(worldPos, 0.1f);

        // Linie de debug Player → Mouse
        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(transform.position, worldPos);
    }
}