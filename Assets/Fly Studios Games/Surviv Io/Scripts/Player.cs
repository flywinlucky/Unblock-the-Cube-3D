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

    // Smooth movement settings
    [Header("Mișcare Smooth")]
    [Tooltip("Timp (s) folosit de SmoothDamp pentru a netezi schimbarea vitezei.")]
    public float velocitySmoothTime = 0.08f;
    [Tooltip("Dacă vrei decelerare/accelerare personalizată, modifică aceste valori.")]
    public float acceleration = 40f;
    public float deceleration = 40f;

    private Vector3 worldPos; // Poziția cursorului în lume

    public Transform weaponRootPosition;
    public WeaponControler weaponController;
    public bool holdToFire = true; // ține apăsat pentru foc continuu

    // intern
    private Rigidbody2D _rb2;
    private Vector2 _inputDir = Vector2.zero;
    private Vector2 _velocityRef = Vector2.zero;
    private Vector2 _currentVelocity = Vector2.zero;

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

        // încercăm să folosim Rigidbody2D dacă există pentru mișcare fizică stabilă
        _rb2 = GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        // citim input-ul, dar aplicarea mișcării se face în FixedUpdate pentru stabilitate
        HandleInput();

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

        // Fire input (mouse left)
        if (weaponController != null)
        {
            if (holdToFire)
            {
                if (Input.GetMouseButton(0))
                    weaponController.Fire();
            }
            else
            {
                if (Input.GetMouseButtonDown(0))
                    weaponController.Fire();
            }

            // Manual reload (R) if magazine is not full and reserve > 0
            if (Input.GetKeyDown(KeyCode.R))
                weaponController.Reload();
        }
    }

    // Citire input separată pentru claritate
    void HandleInput()
    {
        float moveX = Input.GetAxisRaw("Horizontal");
        float moveY = Input.GetAxisRaw("Vertical");
        _inputDir = new Vector2(moveX, moveY);

        // Normalizăm pentru a evita viteza mai mare pe diagonale
        if (_inputDir.sqrMagnitude > 1f)
            _inputDir = _inputDir.normalized;
    }

    // FixedUpdate pentru mișcare fizică stabilă
    void FixedUpdate()
    {
        ApplyMovement();
    }

    void ApplyMovement()
    {
        Vector2 targetVelocity = _inputDir * moveSpeed;

        if (_rb2 != null)
        {
            // Folosim SmoothDamp pe velocity pentru mișcare fluidă.
            // Vector2.SmoothDamp folosește _velocityRef ca referință internă.
            Vector2 newVel = Vector2.SmoothDamp(_rb2.velocity, targetVelocity, ref _velocityRef, velocitySmoothTime);

            // Opțional: limităm accel/decel prin MoveTowards pe fiecare frame pentru senzație mai "greu" sau "moale"
            float maxDelta = (_inputDir.sqrMagnitude > 0.001f ? acceleration : deceleration) * Time.fixedDeltaTime;
            newVel = Vector2.MoveTowards(_rb2.velocity, newVel, maxDelta);

            _rb2.velocity = newVel;
        }
        else
        {
            // fallback fără Rigidbody2D: mutăm transform cu SmoothDamp pe viteză
            Vector2 smoothVel = Vector2.SmoothDamp(_currentVelocity, targetVelocity, ref _velocityRef, velocitySmoothTime);
            transform.position += (Vector3)(smoothVel * Time.fixedDeltaTime);
            _currentVelocity = smoothVel;
        }
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