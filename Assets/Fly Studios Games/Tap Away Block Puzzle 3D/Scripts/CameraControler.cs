using System;
using System.Collections;
using UnityEngine;

namespace Tap_Away_Block_Puzzle_3D
{
    /// <summary>
    /// Controls rotation of a target group (orbit-style) and handles smooth zooming.
    /// Includes a method to frame the target in view.
    /// </summary>
    public class CameraControler : MonoBehaviour
    {
        #region Inspector

        [Tooltip("Parent transform that will be rotated.")]
        public Transform target;

        [Header("Rotation Settings")]
        [Tooltip("Mouse/touch rotation speed.")]
        public float rotationSpeed = 5.0f;
        [Tooltip("Damping used for smoothing rotation input (smaller = snappier).")]
        [Range(0.01f, 0.5f)]
        public float rotationDamping = 0.05f;

        [Header("Zoom Settings")]
        [Tooltip("Zoom sensitivity for mouse scroll and touch pinch.")]
        public float zoomSpeed = 10.0f;
        [Tooltip("Damping used for smoothing zoom (smaller = snappier).")]
        [Range(0.01f, 0.5f)]
        public float zoomDamping = 0.1f;
        [Tooltip("Minimum allowed camera distance.")]
        public float minDistance = 3f;
        [Tooltip("Maximum allowed camera distance.")]
        public float maxDistance = 50f;
        [Tooltip("Extra buffer factor when framing target (e.g. 1.2 = 20% padding).")]
        public float frameBuffer = 1.5f;

        [Tooltip("Enable or disable target rotation (useful when modal UI is open).")]
        public bool rotationEnabled = true;

        [Header("Touch Settings")]
        [Tooltip("Adjustment factor for touch rotation sensitivity.")]
        public float touchRotationSensitivity = 0.5f;
        [Tooltip("Adjustment factor for touch pinch zoom sensitivity.")]
        public float touchZoomSensitivity = 0.1f;

        #endregion

        #region Private State

        private Vector3 _centerPoint;
        private float _currentDistance;
        private float _desiredDistance;
        private Camera _mainCamera;

        private Vector2 _rotationInput;
        private Vector2 _smoothVelocity;

        private Vector2 _touchRotationInput;
        private Vector2 _touchSmoothVelocity;

        private int activeTouchId = -1;
        private float lastTouchDistance;

        #endregion

        #region Unity Lifecycle

        IEnumerator Start()
        {
            _mainCamera = Camera.main;
            if (target == null || _mainCamera == null)
            {
                Debug.LogError("Target or main Camera is not assigned.", this);
                this.enabled = false;
                yield break;
            }

            // Wait one frame to allow child renderers to initialize, then frame the target.
            yield return new WaitForEndOfFrame();
            FrameTarget();
        }

        void LateUpdate()
        {
            if (target)
            {
                HandleInput();
                ApplyZoom();
                transform.LookAt(_centerPoint);

                if (!_hasRotated && DetectRotation())
                {
                    _hasRotated = true;
                    OnObjectRotated?.Invoke();
                }
            }
        }

        #endregion

        #region Framing

        /// <summary>
        /// Instantly calculate and position the camera to fit the entire target group in view.
        /// </summary>
        public void FrameTarget()
        {
            Bounds totalBounds = new Bounds();
            Renderer[] renderers = target.GetComponentsInChildren<Renderer>();

            if (renderers.Length == 0)
            {
                _centerPoint = target.position;
                Debug.LogWarning("Target has no Renderer components. Framing may be imprecise.", target);
                totalBounds = new Bounds(target.position, Vector3.one);
            }
            else
            {
                totalBounds = renderers[0].bounds;
                foreach (Renderer rend in renderers)
                {
                    totalBounds.Encapsulate(rend.bounds);
                }
            }

            _centerPoint = totalBounds.center;

            float objectSize = Mathf.Max(totalBounds.size.x, totalBounds.size.y);
            float cameraView = 2.0f * Mathf.Tan(0.5f * _mainCamera.fieldOfView * Mathf.Deg2Rad);
            float distanceToFit = (objectSize / _mainCamera.aspect) / cameraView;

            float finalDistance = distanceToFit * frameBuffer;
            float targetDistance = Mathf.Clamp(finalDistance, minDistance, maxDistance);

            _desiredDistance = targetDistance;
            _currentDistance = targetDistance;

            transform.position = _centerPoint - transform.forward * _currentDistance;
        }

        #endregion

        #region Input Handling

        private void HandleInput()
        {
            if (Input.touchCount > 0)
            {
                // Touch input (pinch zoom and single-finger rotate)
                if (Input.touchCount == 2)
                {
                    activeTouchId = -1;
                    _rotationInput = Vector2.zero;
                    _smoothVelocity = Vector2.zero;
                    _touchRotationInput = Vector2.zero;
                    _touchSmoothVelocity = Vector2.zero;

                    Touch touch0 = Input.GetTouch(0);
                    Touch touch1 = Input.GetTouch(1);
                    float currentTouchDistance = Vector2.Distance(touch0.position, touch1.position);

                    if (touch0.phase == TouchPhase.Began || touch1.phase == TouchPhase.Began)
                    {
                        lastTouchDistance = currentTouchDistance;
                    }
                    else if (touch0.phase == TouchPhase.Moved || touch1.phase == TouchPhase.Moved)
                    {
                        float distanceDelta = currentTouchDistance - lastTouchDistance;
                        _desiredDistance -= distanceDelta * touchZoomSensitivity;
                        _desiredDistance = Mathf.Clamp(_desiredDistance, minDistance, maxDistance);
                        lastTouchDistance = currentTouchDistance;
                    }
                }
                else if (Input.touchCount == 1)
                {
                    if (!rotationEnabled) return;

                    Touch touch = Input.GetTouch(0);
                    Vector2 rawTouchInput = Vector2.zero;

                    if (activeTouchId == -1)
                        activeTouchId = touch.fingerId;

                    if (touch.fingerId == activeTouchId)
                    {
                        if (touch.phase == TouchPhase.Moved)
                        {
                            rawTouchInput = touch.deltaPosition * touchRotationSensitivity;
                        }
                        else if (touch.phase == TouchPhase.Ended || touch.phase == TouchPhase.Canceled)
                        {
                            activeTouchId = -1;
                        }
                    }

                    _touchRotationInput = Vector2.SmoothDamp(_touchRotationInput, rawTouchInput, ref _touchSmoothVelocity, rotationDamping);

                    if (_touchRotationInput.magnitude > 0.001f)
                    {
                        target.RotateAround(_centerPoint, Vector3.up, _touchRotationInput.x * rotationSpeed * Time.deltaTime);
                        target.RotateAround(_centerPoint, transform.right, -_touchRotationInput.y * rotationSpeed * Time.deltaTime);
                    }
                }
            }
            else
            {
                // Mouse input fallback
                activeTouchId = -1;
                _touchRotationInput = Vector2.SmoothDamp(_touchRotationInput, Vector2.zero, ref _touchSmoothVelocity, rotationDamping);

                if (rotationEnabled)
                {
                    Vector2 rawInput = Vector2.zero;
                    if (Input.GetMouseButton(0) || Input.GetMouseButton(1))
                    {
                        rawInput = new Vector2(Input.GetAxis("Mouse X"), Input.GetAxis("Mouse Y"));
                    }

                    _rotationInput = Vector2.SmoothDamp(_rotationInput, rawInput, ref _smoothVelocity, rotationDamping);

                    if (_rotationInput.magnitude > 0.001f)
                    {
                        target.RotateAround(_centerPoint, Vector3.up, -_rotationInput.x * rotationSpeed);
                        target.RotateAround(_centerPoint, transform.right, _rotationInput.y * rotationSpeed);
                    }
                }

                float scroll = Input.GetAxis("Mouse ScrollWheel");
                if (Mathf.Abs(scroll) > 0.01f)
                {
                    _desiredDistance -= scroll * zoomSpeed;
                    _desiredDistance = Mathf.Clamp(_desiredDistance, minDistance, maxDistance);
                }
            }
        }

        private void ApplyZoom()
        {
            _currentDistance = Mathf.Lerp(_currentDistance, _desiredDistance, zoomDamping);

            Vector3 directionFromCenter = (transform.position - _centerPoint).normalized;
            if (directionFromCenter == Vector3.zero) directionFromCenter = -transform.forward;
            transform.position = _centerPoint + directionFromCenter * _currentDistance;
        }

        #endregion

        #region Rotation Detection Event

        /// <summary>
        /// Raised when the user rotates the object for the first time (used by tutorial).
        /// </summary>
        public static event Action OnObjectRotated;

        private bool _hasRotated = false;

        private bool DetectRotation()
        {
            return Mathf.Abs(_rotationInput.x) > 0.01f || Mathf.Abs(_rotationInput.y) > 0.01f ||
                   Mathf.Abs(_touchRotationInput.x) > 0.01f || Mathf.Abs(_touchRotationInput.y) > 0.01f;
        }

        /// <summary>
        /// Reset the rotation-detected flag so the event can be fired again.
        /// </summary>
        public void ResetRotationFlag()
        {
            _hasRotated = false;
        }

        #endregion
    }
}