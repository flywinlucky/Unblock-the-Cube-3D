using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    [Header("Setări Cameră și Rotație")]
    public Camera cam;
    public float zDepth = 0f; // Adâncimea Z la care se calculează cursorul

    [Header("Setări Mișcare")]
    public float moveSpeed = 5f; // Viteza de mișcare

    private Vector3 worldPos; // Poziția cursorului în lume

    public Transform weaponRootPosition;

    void Start()
    {
        // Încearcă să găsească camera principală automat dacă nu e setată
        if (cam == null)
        {
            cam = Camera.main;
        }
        
        if (cam == null)
        {
            Debug.LogError("Player.cs: 'cam' nu este setată și 'Camera.main' nu a fost găsită!");
        }
    }

    void Update()
    {
        HandleMovement();

        // Oprim dacă nu avem o cameră setată
        if (cam == null)
        {
            return;
        }

        // Convertim mouse-ul în poziție din lume
        Vector3 mouse = Input.mousePosition;
        // Acest calcul specific depinde de cum e setată camera (ortografică sau perspectivă)
        mouse.z = -cam.transform.position.z + zDepth; 
        worldPos = cam.ScreenToWorldPoint(mouse);

        // Direcția din player către mouse
        Vector2 direction = worldPos - transform.position;

        // Calculăm unghiul
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

        transform.rotation = Quaternion.Euler(0f, 0f, angle);
    }

    void HandleMovement()
    {
        float moveX = Input.GetAxisRaw("Horizontal"); 
        float moveY = Input.GetAxisRaw("Vertical");  

        Vector2 moveDirection = new Vector2(moveX, moveY);
        moveDirection.Normalize();

        transform.position += (Vector3)moveDirection * moveSpeed * Time.deltaTime;
    }

    // Funcția de debug din noul tău script
    void OnDrawGizmos()
    {
        if (cam == null)
            cam = Camera.main;
        
        if (cam == null) return;

        Gizmos.color = Color.red;
        Gizmos.DrawSphere(worldPos, 0.1f);

        // Linie de debug Player → Mouse
        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(transform.position, worldPos);
    }
}