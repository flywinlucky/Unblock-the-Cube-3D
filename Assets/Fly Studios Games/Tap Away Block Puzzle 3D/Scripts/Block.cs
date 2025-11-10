using UnityEngine;
using System;
using System.Collections;

namespace Tap_Away_Block_Puzzle_3D
{
    /// <summary>
    /// Represents a single block in the level. Handles input, movement, visual effects and destruction.
    /// Public API: Initialize, Smash, FlashHint.
    /// </summary>
    public class Block : MonoBehaviour
    {
        #region Inspector - Animation / Visuals

        [Header("Animation Settings")]
        [Tooltip("Duration of the dissolve (scale-out) animation in seconds.")]
        [HideInInspector]
        public float dissolveDuration = 0.2f;

        [Header("Default Data")]
        [Tooltip("Default skin data used when no shop skin is selected.")]
        public ShopSkinData defaultSkin; // Read default skin data from ShopSkinData

        [HideInInspector]
        public Color arrowCollor;

        [Tooltip("Arrow renderers used to tint the direction arrows.")]
        public MeshRenderer[] cubeArows;

        [Tooltip("Main cube mesh renderer used to apply skins.")]
        public MeshRenderer cubeMesh;

        #endregion

        #region Private Fields

        private MoveDirection _moveDirection;
        private LevelManager _levelManager;
        private bool _isMoving = false;
        private BoxCollider _collider;
        private float _gridUnitSize;
        private bool _isShaking = false;

        [HideInInspector]
        public bool _isInteractible;

        // Event invoked when this block is activated (clicked)
        public static event Action<Block> OnBlockActivated;

        // Grid position cached for potential undo / bookkeeping
        private Vector3Int _gridPosition;

        #endregion

        #region MonoBehaviour

        private void Awake()
        {
            _collider = GetComponent<BoxCollider>();
            _isInteractible = true;
        }

        #endregion

        #region Initialization / Skinning

        /// <summary>
        /// Initialize the block with direction, level manager reference, grid size and grid position.
        /// </summary>
        public void Initialize(MoveDirection dir, LevelManager manager, float gridUnitSize, Vector3Int gridPosition)
        {
            _moveDirection = dir;
            _levelManager = manager;
            _gridUnitSize = gridUnitSize;
            _gridPosition = gridPosition;

            // Apply currently selected skin or defaults
            if (_levelManager != null && _levelManager.shopManager != null && _levelManager.shopManager.selectedSkin != null)
            {
                ShopSkinData currentSkin = _levelManager.shopManager.selectedSkin;
                ApplySkin(currentSkin.material);
                arrowCollor = currentSkin.arrowColor;
            }
            else if (defaultSkin != null)
            {
                ApplySkin(defaultSkin.material);
                arrowCollor = defaultSkin.arrowColor;
            }

            ApplyArrowColor();
        }

        #endregion

        #region Input Handling

        private void OnMouseUpAsButton()
        {
            if (_isMoving || !_isInteractible) return;

            // Fire block activated event
            OnBlockActivated?.Invoke(this);

            // If in remove mode, confirm removal and exit
            if (_levelManager != null && _levelManager.IsAwaitingRemove())
            {
                _levelManager.ConfirmRemoveAtBlock(this);
                return;
            }

            // Play click sound via AudioManager if available
            if (_levelManager != null && _levelManager.audioManager != null)
            {
                _levelManager.audioManager.PlayBlockClick();
            }

            Vector3 startPos = transform.position;
            Vector3 direction = transform.forward;
            Vector3 targetPosition = startPos;
            bool shouldBeDestroyed = false;

            // Calculate the furthest valid position in the grid
            while (true)
            {
                Vector3 nextPosition = targetPosition + direction * _gridUnitSize;
                if (Physics.Raycast(targetPosition, direction, out RaycastHit hit, _gridUnitSize))
                {
                    // Stop at the obstacle
                    break;
                }

                targetPosition = nextPosition;

                // If we exceed a safety limit, treat as destroyed (keeps original behavior)
                if (Vector3.Distance(startPos, targetPosition) > 10f) // Example safety limit of 10 grid units
                {
                    shouldBeDestroyed = true;
                    break;
                }
            }

            if (Vector3.Distance(transform.position, targetPosition) > 0.1f)
            {
                _isMoving = true;
                if (shouldBeDestroyed)
                {
                    if (_collider != null) _collider.enabled = false;
                    _levelManager?.OnBlockRemoved(this);
                }
                StartCoroutine(MoveWithDamping(targetPosition, shouldBeDestroyed));
            }
            else
            {
                StartCoroutine(ShakeScale());
            }

            // Check for available moves after the block finishes moving
            _levelManager?.CheckForAvailableMoves();
        }

        #endregion

        #region Movement & Animations

        // Movement coroutine - motion with damping/lerp. Behavior preserved.
        private IEnumerator MoveWithDamping(Vector3 targetPosition, bool shouldBeDestroyed)
        {
            Vector3 startPosition = transform.position;
            float duration = Vector3.Distance(startPosition, targetPosition) * 0.1f;
            float elapsed = 0f;

            while (elapsed < duration)
            {
                transform.position = Vector3.Lerp(startPosition, targetPosition, Mathf.SmoothStep(0, 1, elapsed / duration));
                elapsed += Time.deltaTime;
                yield return null;
            }

            transform.position = targetPosition;

            if (shouldBeDestroyed)
            {
                yield return StartCoroutine(ScaleOutAndDestroy());
            }
            else
            {
                yield return StartCoroutine(ImpactBounce());
                _isMoving = false;
            }
        }

        /// <summary>
        /// Smoothly scale the block to zero and destroy the GameObject.
        /// </summary>
        private IEnumerator ScaleOutAndDestroy()
        {
            Vector3 originalScale = transform.localScale;
            Vector3 targetScale = Vector3.zero;
            float elapsed = 0f;

            while (elapsed < dissolveDuration)
            {
                transform.localScale = Vector3.Lerp(originalScale, targetScale, elapsed / dissolveDuration);
                elapsed += Time.deltaTime;
                yield return null;
            }

            Destroy(gameObject);
        }

        private IEnumerator ShakeScale()
        {
            if (_isShaking) yield break;
            _isShaking = true;
            Vector3 originalScale = transform.localScale;
            Vector3 targetScale = originalScale * 1.15f;
            float duration = 0.1f;
            float elapsed = 0f;
            while (elapsed < duration)
            {
                transform.localScale = Vector3.Lerp(originalScale, targetScale, elapsed / duration);
                elapsed += Time.deltaTime;
                yield return null;
            }
            elapsed = 0f;
            while (elapsed < duration)
            {
                transform.localScale = Vector3.Lerp(targetScale, originalScale, elapsed / duration);
                elapsed += Time.deltaTime;
                yield return null;
            }
            transform.localScale = originalScale;
            _isShaking = false;
        }

        private IEnumerator ImpactBounce()
        {
            if (_isShaking) yield break;
            _isShaking = true;
            Vector3 originalPos = transform.position;
            Vector3 bouncePos = originalPos - transform.forward * 0.05f;
            float duration = 0.08f;
            float elapsed = 0f;
            while (elapsed < duration)
            {
                transform.position = Vector3.Lerp(originalPos, bouncePos, Mathf.SmoothStep(0, 1, elapsed / duration));
                elapsed += Time.deltaTime;
                yield return null;
            }
            elapsed = 0f;
            while (elapsed < duration)
            {
                transform.position = Vector3.Lerp(bouncePos, originalPos, Mathf.SmoothStep(0, 1, elapsed / duration));
                elapsed += Time.deltaTime;
                yield return null;
            }
            transform.position = originalPos;
            _isShaking = false;
        }

        #endregion

        #region Public API - Smash / Hints / Skin

        /// <summary>
        /// Instantly destroy this block with the dissolve animation. Used by Smash power-up.
        /// </summary>
        public void Smash()
        {
            if (_isMoving) return;
            if (_collider != null) _collider.enabled = false;
            _levelManager?.OnBlockRemoved(this);
            StartCoroutine(ScaleOutAndDestroy());
        }

        /// <summary>
        /// Visual hint: briefly scale the block to draw attention.
        /// </summary>
        public void FlashHint(float duration = 0.6f, float scaleFactor = 1.25f)
        {
            StartCoroutine(FlashHintCoroutine(duration, scaleFactor));
        }

        private IEnumerator FlashHintCoroutine(float duration, float scaleFactor)
        {
            Vector3 originalScale = transform.localScale;
            Vector3 targetScale = originalScale * scaleFactor;
            float half = duration * 0.5f;
            float t = 0f;
            while (t < half)
            {
                transform.localScale = Vector3.Lerp(originalScale, targetScale, t / half);
                t += Time.deltaTime;
                yield return null;
            }
            t = 0f;
            while (t < half)
            {
                transform.localScale = Vector3.Lerp(targetScale, originalScale, t / half);
                t += Time.deltaTime;
                yield return null;
            }
            transform.localScale = originalScale;
        }

        /// <summary>
        /// Apply a skin material to the block. Creates a new material instance to avoid global shared-material issues.
        /// </summary>
        public void ApplySkin(Material mat)
        {
            if (mat != null)
            {
                if (cubeMesh != null)
                {
                    cubeMesh.material = new Material(mat);
                }
            }

            ApplyArrowColor();
        }

        /// <summary>
        /// Apply the arrow color to arrow renderers. Creates material instances if missing.
        /// </summary>
        public void ApplyArrowColor()
        {
            if (cubeArows == null || cubeArows.Length == 0) return;

            foreach (var arrowRenderer in cubeArows)
            {
                if (arrowRenderer == null) continue;

                if (arrowRenderer.material == null)
                {
                    Debug.LogWarning("Arrow material is missing. Creating a new material instance.");
                    arrowRenderer.material = new Material(Shader.Find("Custom/UnlitTransparentColor"));
                }

                arrowRenderer.material.SetColor("_Color", arrowCollor);
                arrowRenderer.material.SetFloat("_Alpha", 1f);
            }
        }

        #endregion

        #region Debug / Helpers

        private Vector3 GetWorldDirection()
        {
            switch (_moveDirection)
            {
                case MoveDirection.Forward: return Vector3.forward;
                case MoveDirection.Back: return Vector3.back;
                case MoveDirection.Up: return Vector3.up;
                case MoveDirection.Down: return Vector3.down;
                case MoveDirection.Left: return Vector3.left;
                case MoveDirection.Right: return Vector3.right;
            }
            return Vector3.zero;
        }

        private void OnDrawGizmos()
        {
            if (!Application.isPlaying) return;
            Vector3 direction = transform.forward;
            RaycastHit hit;
            if (Physics.Raycast(transform.position, direction, out hit, 100f))
            {
                Gizmos.color = Color.red;
                Gizmos.DrawLine(transform.position, hit.point);
            }
            else
            {
                Gizmos.color = Color.green;
                Gizmos.DrawRay(transform.position, direction * 10f);
            }
        }

        #endregion
    }
}